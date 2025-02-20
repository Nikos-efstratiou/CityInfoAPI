using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.DbContexts
{
    public class CityInfoContext : DbContext
    {
        public DbSet<City> Cities { get; set; }

        public DbSet<PointOfInterest> PointsOfInterest { get; set; }

        public CityInfoContext(DbContextOptions<CityInfoContext> options)
           : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // First, seed the Cities table
            modelBuilder.Entity<City>().HasData(
                new City("New York City")
                {
                    Id = 1, // Ensure this matches the CityId in PointOfInterest
                    Description = "The one with the big park"
                }
            );

            // Then, seed the PointsOfInterest table
            modelBuilder.Entity<PointOfInterest>().HasData(
                new PointOfInterest("Central Park")
                {
                    Id = 1, // Unique Id for PointOfInterest
                    Description = "A large public park in NYC",
                    CityId = 1 // Must match an existing City Id
                }
            );

            base.OnModelCreating(modelBuilder);
        }



        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("connectionstring");
        //    base.OnConfiguring(optionsBuilder);
        // }

    }
}
