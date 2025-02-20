namespace CityInfo.API.Models
{
    public class PointOfInterestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public int NumberOfPointsOfInterest
        {
            get
            {
                return PointOfInterest.Count;
            }


        }

        public ICollection<PointOfInterestDto> PointOfInterest { get; set;}
            = new List<PointOfInterestDto>();



    }
}
