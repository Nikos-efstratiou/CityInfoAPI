using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Services.WebApi.Patch;

namespace CityInfo.API.Controllers
{
    [Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
    [ApiVersion(2)]
    [Authorize]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {

        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly IMapper _mapper;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly CitiesDataStore _citiesDataStore;
        public PointsOfInterestController(
     ILogger<PointsOfInterestController> logger,
     IMailService mailService,
     ICityInfoRepository cityInfoRepository,IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _mapper = mapper;
         _cityInfoRepository = cityInfoRepository;
            

            // 🔥 DEBUG: Check if CitiesDataStore is actually populated
            Console.WriteLine($"Cities count: {_citiesDataStore.Cities.Count}");
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterestint(int cityId)

        {
            var CityName = User.Claims.FirstOrDefault(c=>c.Type=="city")?.Value;
           if(! await _cityInfoRepository.CityNameMatchesCityId(CityName,cityId))
            {

                return Forbid();
            }

            if (!await _cityInfoRepository.CityExistsAsync(cityId))

            {
                _logger.LogInformation(

                   $"City with id {cityId} wasnt found"
                    );
                return NotFound();
            
            }


            var pointsOfInterestForCity = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
            return Ok();
          
        }


        [HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]

        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(
            int cityId, int pointOfInterestId)
        { 
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {

                return NotFound();

            }
            var pointOfInterest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

            if (pointOfInterest == null)
            {

                return NotFound();
            
            }
            return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(
       int cityId,
       [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
           if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            // ✅ Handle empty list safely

            var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);
           await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);

            await _cityInfoRepository.SaveChangesAsync();
            var CreatedPointOfInterestToReturn = _mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);


            // ✅ Ensure the GET method has the correct route name
            return CreatedAtRoute(
                "GetPointOfInterest", // 🔥 Ensure your GET method has this name!
                new
                {
                    cityId = cityId,
                    pointOfInterestId = CreatedPointOfInterestToReturn.Id
                },
               CreatedPointOfInterestToReturn
            );
        }

        [HttpPut("{pointOfInterestId}")]

        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
           if (!await _cityInfoRepository.CityExistsAsync(cityId) )
            {
                return NotFound(); 

            }




            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(pointOfInterestId, pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();

          
         

            return NoContent(); // 204 No Content (Success, but no data returned)
        }
        //    [HttpPatch("{pointOfInterestId}")]

        //    public ActionResult PartiallyUpdatePointOfInterest(
        //        int cityId, int pointOfInterestId,JsonPatchDocument<PointOfInterestForUpdateDto>patchDocument

        //        )

        //    {
        //        var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
        //        if (city == null)
        //        {
        //            return NotFound();
        //        }
        //        var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointOfInterestId);
        //        if (pointOfInterestFromStore == null)
        //        {
        //            return NotFound();
        //        }
        //        var pointOfInterestToPatch =
        //            new PointOfInterestForUpdateDto()
        //            {
        //                Name = pointOfInterestFromStore.Name,
        //                Description = pointOfInterestFromStore.Description

        //            };
        //        patchDocument.ApplyTo(pointOfInterestToPatch,ModelState);

        //        if(!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }
        //        return NoContent();

        //    }

        [HttpDelete("{pointOfInterestId}")]

        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)

        { if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {

                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId); 
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();
            
            
            _mailService.Send("Point of interest deleted.",
                $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");



            return NoContent();
        }

    }
}
