using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/v{version:apiVersion}/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {

        [HttpGet("{fileId}")]
        [ApiVersion(0.1, Deprecated = true)]
        public ActionResult GetFile(string fileId)
        {
            var pathToFile = "e-cert.pdf";
            var bytes = System.IO.File.ReadAllBytes(pathToFile);
            return File(bytes, "text/plain", Path.GetFileName(pathToFile));
        }

        [HttpPost]

        public async Task<ActionResult> CreateFile(IFormFile file) 
        {

            var path = Path.Combine(

                Directory.GetCurrentDirectory(),
                $"uploaded_file_{Guid.NewGuid()}.pdf");
            using ( var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);

            }
            return Ok("your file was uploaded");
        }

    }
}
