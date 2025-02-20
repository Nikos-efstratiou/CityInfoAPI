using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [ApiVersion(1)]
    [ApiVersion(2)]
     [Authorize]
    [Route("api/v{version:apiVersion}/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        const int maxCitiesPageSize = 20;

        public CitiesController(ICityInfoRepository cityInfoRepository,IMapper mapper)
        {
            // No need to throw an exception if cityInfoRepository is null
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(_mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> 
            GetCities(string? name,string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxCitiesPageSize) 
            {
            pageSize = maxCitiesPageSize;
            
            }

            var cityEntities = await _cityInfoRepository.GetCitiesAsync(name,searchQuery,pageNumber,pageSize);
            

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));
        }
        /// <summary>
        /// Get a city by id
        /// </summary>
        /// <param name="id">the id of the city to get</param>
        /// <param name="inludePointsOfInterest">whether or not to include points of interest</param>
        /// <returns>A city with or without points of interest</returns>
        [HttpGet("{cityId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCity(int cityId,bool inludePointsOfInterest=false)
        {
            // Assuming you're going to fetch a city based on the id
            var city = await _cityInfoRepository.GetCityAsync(cityId, inludePointsOfInterest);
            if (city==null)
            {
                return NotFound();
            }
            if(inludePointsOfInterest)
            { 
            return Ok(_mapper.Map<CityDto>(city));
            
            }
            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));

            
        }
    }
}
