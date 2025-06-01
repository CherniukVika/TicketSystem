using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.BLL.Dto;
using TicketSystem.BLL.Services;
using TicketSystem.BLL.Strategy;
using TicketSystem.PL.Models;

namespace TicketSystem.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITheaterService _theaterService;
        private readonly IMapper _mapper;

        public TicketController(ITheaterService theaterService, IMapper mapper)
        {
            _theaterService = theaterService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetTicketsByStatus([FromQuery] string status)
        {
            if (string.IsNullOrWhiteSpace(status) || !new[] { "Sold", "Returned" }.Contains(status))
            {
                return BadRequest(new { Message = "Неприпустимий статус. Використовуйте 'Sold' або 'Returned'." });
            }

            var ticketStatus = status switch
            {
                "Sold" => TicketStatus.Sold,
                "Returned" => TicketStatus.Returned,
                _ => throw new ArgumentException($"Некоректний статус: {status}")
            };

            var tickets = await _theaterService.GetTicketsByStatusAsync(ticketStatus);
            var ticketPLDtos = new List<TicketPLDto>();

            foreach (var ticket in tickets)
            {
                var seat = (await _theaterService.GetAvailableSeatsAsync(ticket.PerformanceId, ticket.PerformanceScheduleId, null))
                    .FirstOrDefault(s => s.Id == ticket.SeatId);

                ticketPLDtos.Add(new TicketPLDto
                {
                    Id = ticket.Id,
                    PerformanceId = ticket.PerformanceId,
                    PerformanceScheduleId = ticket.PerformanceScheduleId,
                    SeatId = ticket.SeatId,
                    SeatLocation = seat?.Location ?? "Unknown",
                    Price = ticket.Price,
                    PurchaseDate = ticket.PurchaseDate,
                    PhoneNumber = ticket.PhoneNumber,
                    Status = ticket.Status.ToString()
                });
            }

            return Ok(ticketPLDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var tickets = await _theaterService.GetTicketsByStatusAsync(TicketStatus.Sold);
            var ticket = tickets.FirstOrDefault(t => t.Id == id);
            if (ticket == null)
                return NotFound();

            var seat = (await _theaterService.GetAvailableSeatsAsync(ticket.PerformanceId, ticket.PerformanceScheduleId, null))
                .FirstOrDefault(s => s.Id == ticket.SeatId);

            var ticketPLDto = new TicketPLDto
            {
                Id = ticket.Id,
                PerformanceId = ticket.PerformanceId,
                PerformanceScheduleId = ticket.PerformanceScheduleId,
                SeatId = ticket.SeatId,
                SeatLocation = seat?.Location ?? "Unknown",
                Price = ticket.Price,
                PurchaseDate = ticket.PurchaseDate,
                PhoneNumber = ticket.PhoneNumber,
                Status = ticket.Status.ToString()
            };
            return Ok(ticketPLDto);
        }

        [HttpPost]
        public async Task<IActionResult> BuyTicket([FromBody] BuyTicketRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            ITicketPricingStrategy pricingStrategy = request.Location switch
            {
                "Hall" => new HallPricingStrategy(),
                "Balcony" => new BalconyPricingStrategy(),
                _ => throw new ArgumentException("Некоректна локація")
            };

            try
            {
                var ticket = await _theaterService.BuyTicketAsync(
                    request.PerformanceId,
                    request.ScheduleId,
                    request.SeatId,
                    pricingStrategy,
                    request.PhoneNumber);

                if (ticket == null)
                    return BadRequest("Не вдалося придбати квиток: місце недоступне або некоректні дані.");

                var seat = (await _theaterService.GetAvailableSeatsAsync(ticket.PerformanceId, ticket.PerformanceScheduleId, null))
                    .FirstOrDefault(s => s.Id == ticket.SeatId);

                var ticketPLDto = new TicketPLDto
                {
                    Id = ticket.Id,
                    PerformanceId = ticket.PerformanceId,
                    PerformanceScheduleId = ticket.PerformanceScheduleId,
                    SeatId = ticket.SeatId,
                    SeatLocation = seat?.Location ?? "Unknown",
                    Price = ticket.Price,
                    PurchaseDate = ticket.PurchaseDate,
                    PhoneNumber = ticket.PhoneNumber,
                    Status = ticket.Status.ToString()
                };
                return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, ticketPLDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnTicket(int id, [FromBody] string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest(new { Message = "Номер телефону є обов'язковим." });

            var ticket = await _theaterService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound(new { Message = $"Квиток з ID {id} не знайдено." });

            if (ticket.Status == TicketStatus.Returned)
                return BadRequest(new { Message = $"Квиток з ID {id} уже повернено." });

            if (ticket.PhoneNumber != phoneNumber)
                return BadRequest(new { Message = "Введений номер телефону не збігається з номером, указаним у квитку." });

            var (success, updatedTicket) = await _theaterService.ReturnTicketAsync(id, phoneNumber);
            if (!success || updatedTicket == null)
                return BadRequest(new { Message = "Не вдалося повернути квиток: можливо, минув період повернення або квиток некоректний." });

            var seat = (await _theaterService.GetAvailableSeatsAsync(updatedTicket.PerformanceId, updatedTicket.PerformanceScheduleId, null))
                .FirstOrDefault(s => s.Id == updatedTicket.SeatId);

            var ticketPLDto = new TicketPLDto
            {
                Id = updatedTicket.Id,
                PerformanceId = updatedTicket.PerformanceId,
                PerformanceScheduleId = updatedTicket.PerformanceScheduleId,
                SeatId = updatedTicket.SeatId,
                SeatLocation = seat?.Location ?? "Unknown",
                Price = updatedTicket.Price,
                PurchaseDate = updatedTicket.PurchaseDate,
                PhoneNumber = updatedTicket.PhoneNumber,
                Status = updatedTicket.Status.ToString()
            };

            return Ok(new { Message = "Квиток успішно повернено", RefundAmount = ticketPLDto.Price, Ticket = ticketPLDto });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _theaterService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound(new { Message = $"Квиток з ID {id} не знайдено." });

            if (ticket.Status == TicketStatus.Returned)
                return BadRequest(new { Message = $"Квиток з ID {id} уже повернено." });

            var (success, updatedTicket) = await _theaterService.ReturnTicketAsync(id, ticket.PhoneNumber);
            if (!success || updatedTicket == null)
                return BadRequest(new { Message = $"Не вдалося повернути квиток з ID {id}: можливо, минув період повернення або квиток некоректний." });

            var seat = (await _theaterService.GetAvailableSeatsAsync(updatedTicket.PerformanceId, updatedTicket.PerformanceScheduleId, null))
                .FirstOrDefault(s => s.Id == updatedTicket.SeatId);

            var ticketPLDto = new TicketPLDto
            {
                Id = updatedTicket.Id,
                PerformanceId = updatedTicket.PerformanceId,
                PerformanceScheduleId = updatedTicket.PerformanceScheduleId,
                SeatId = updatedTicket.SeatId,
                SeatLocation = seat?.Location ?? "Unknown",
                Price = updatedTicket.Price,
                PurchaseDate = updatedTicket.PurchaseDate,
                PhoneNumber = updatedTicket.PhoneNumber,
                Status = updatedTicket.Status.ToString()
            };

            return Ok(new { Message = "Квиток успішно повернено", RefundAmount = ticketPLDto.Price, Ticket = ticketPLDto });
        }
    }
}
