namespace CityInfo.API.Models
{
    public class CitiesDataStore
    {
        public List<CityDto> Cities { get; set; }

       // public static CitiesDataStore Current { get; } = new CitiesDataStore();
        public CitiesDataStore() 
        {
        Cities = new List<CityDto>();

            Cities.Add(new CityDto()

            {
                Id = 1,
                Name = "Athens",
                Description = "xalia",
                PointsOfInterest = new List<PointOfInterestDto>()
                { 
                    new PointOfInterestDto()
                    {
                        Id = 1,
                        Name ="neo hrakleio",
                        Description ="xali mavro"
                    }
                }
            });
        
        }

    }
}
