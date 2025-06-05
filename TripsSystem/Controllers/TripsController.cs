using Microsoft.AspNetCore.Mvc;
using TripsSystem.DTOs;
using TripsSystem.Services;

namespace TripsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            if (page.HasValue && pageSize.HasValue)
            {
                if (page <= 0 || pageSize <= 0)
                    return BadRequest("Page number and page size must be greater than 0.");

                var pagedResult = await _tripService.GetTrips(page.Value, pageSize.Value);
                return Ok(pagedResult);
            }
            
            var allTrips = await _tripService.GetAllTrips();
            return Ok(allTrips);
        }

        [HttpPost("{idTrip}/clients")]
        public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] AssignClientDTO dto)
        {
            try
            {
                await _tripService.AssignClientToTrip(idTrip, dto);
                return Ok(new { message = "Client has been successfully assigned to the trip." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}