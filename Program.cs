using System.Reflection;
using Asp.Versioning.ApiExplorer;
using CityInfo.API.DbContexts;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
})
.AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAuthentication("Bearer").AddJwtBearer(options=>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Authentication:Issuer"],
        ValidAudience = builder.Configuration["Authentication:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"]))
    };
}
);
// Swagger setup
builder.Services.AddEndpointsApiExplorer();



// File extension content provider
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

// Conditional mail service setup based on environment
#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

// Problem details configuration (optional)
builder.Services.AddProblemDetails();

// Register repository and data store services
builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();
builder.Services.AddSingleton<CitiesDataStore>();

// Database context configuration
builder.Services.AddDbContext<CityInfoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CityInfoConnection")));  // Updated connection string usage
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.ReportApiVersions=true;
setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new Asp.Versioning.ApiVersion(1.0);
}
).AddMvc()
.AddApiExplorer(setupAction=>
{
setupAction.SubstituteApiVersionInUrl=true; 
}
);

var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider()
    .GetRequiredService<IApiVersionDescriptionProvider>();

builder.Services.AddSwaggerGen(setupAction =>
{
    foreach(var description  in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {

        setupAction.SwaggerDoc(
            $"{description.GroupName}",
            new()
            {
                Title = "City Info Api",
                Version = description.ApiVersion.ToString(),
                Description = "Through this api you can access cities and theri points of interest"
            });

    }



    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    setupAction.IncludeXmlComments(xmlCommentsFullPath);

    setupAction.AddSecurityDefinition("CityInfoApiBearerAuth", new()
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid token to access this API"

    }

        );

    setupAction.AddSecurityRequirement(new()
    {
        {new()

        {
             Reference = new OpenApiReference{
            Type = ReferenceType.SecurityScheme,
            Id = "CityInfoApiBearerAuth"}
        },
        new List<string>()
        }
    }


        );

}
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(setupAction=>
    {
        var descriptions = app.DescribeApiVersions();

        foreach(var description in descriptions)
        {
            setupAction.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant()); 
        }

    }
    
    );
}
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Map controllers
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
