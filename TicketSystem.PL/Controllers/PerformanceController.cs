using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.BLL.Services;
using TicketSystem.PL.Models;

namespace TicketSystem.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerformanceController : ControllerBase
    {
        private readonly ITheaterService _theaterService;
        private readonly IMapper _mapper;

        public PerformanceController(ITheaterService theaterService, IMapper mapper)
        {
            _theaterService = theaterService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPerformances()
        {
            var performances = await _theaterService.GetAllPerformancesAsync();
            var performancePLDtos = _mapper.Map<List<PerformancePLDto>>(performances);
            return Ok(performancePLDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPerformanceById(int id)
        {
            var performance = (await _theaterService.GetAllPerformancesAsync()).FirstOrDefault(p => p.Id == id);
            if (performance == null)
                return NotFound();

            var performancePLDto = _mapper.Map<PerformancePLDto>(performance);
            return Ok(performancePLDto);
        }

        [HttpGet("{performanceId}/schedules/{scheduleId}/seats")]
        public async Task<IActionResult> GetAvailableSeats(int performanceId, int scheduleId, [FromQuery] string location)
        {
            if (!string.IsNullOrEmpty(location) && location != "Hall" && location != "Balcony")
            {
                return BadRequest(new { Message = "Неприпустима локація. Має бути 'Hall' або 'Balcony'." });
            }

            var allPerformances = await _theaterService.GetAllPerformancesAsync();
            if (!allPerformances.Any(p => p.Id == performanceId))
            {
                return NotFound(new { Message = $"Виставу з ID {performanceId} не знайдено." });
            }

            var seats = await _theaterService.GetAvailableSeatsAsync(performanceId, scheduleId, location);

            if (!seats.Any())
            {
                var allSchedulesForPerformance = allPerformances.First(p => p.Id == performanceId).Schedules;
                if (allSchedulesForPerformance != null && allSchedulesForPerformance.Any())
                {
                    var anySeatsForPerformance = false;
                    foreach (var schedule in allSchedulesForPerformance)
                    {
                        var seatsForOtherSchedule = await _theaterService.GetAvailableSeatsAsync(performanceId, schedule.Id, location);
                        if (seatsForOtherSchedule.Any())
                        {
                            anySeatsForPerformance = true;
                            break;
                        }
                    }

                    if (anySeatsForPerformance)
                    {
                        return NotFound(new { Message = $"Розклад з ID {scheduleId} для вистави з ID {performanceId} не знайдено." });
                    }
                }

                return Ok(new { Message = "Доступні місця за вказаними критеріями не знайдено.", Seats = seats });
            }

            return Ok(seats);
        }
    }
}