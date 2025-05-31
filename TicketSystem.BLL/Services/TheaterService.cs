using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TicketSystem.BLL.Dto;
using TicketSystem.BLL.Strategy;
using TicketSystem.DAL.Models;
using TicketSystem.DAL.UnitOfWork;

namespace TicketSystem.BLL.Services
{
    public class TheaterService : ITheaterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TheaterService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<string> GetAuthorNameAsync(int performanceId)
        {
            var performance = await _unitOfWork.Performances.FirstOrDefaultAsync(
                p => p.Id == performanceId,
                query => query.Include(p => p.Author));

            return performance?.Author?.Name ?? "Невідомий автор";
        }

        public async Task<List<string>> GetGenreNamesAsync(int performanceId)
        {
            var performance = await _unitOfWork.Performances.FirstOrDefaultAsync(
                p => p.Id == performanceId,
                query => query.Include(p => p.Genres));

            return performance?.Genres.Select(g => g.Name).ToList() ?? new List<string>();
        }

        public async Task<List<PerformanceDto>> GetAllPerformancesAsync()
        {
            var performances = await _unitOfWork.Performances.GetWithIncludeAsync(
                query => query.Include(p => p.Author),
                query => query.Include(p => p.Genres),
                query => query.Include(p => p.Schedules),
                query => query.Include(p => p.Tickets).ThenInclude(t => t.Seat)
            );

            return _mapper.Map<List<PerformanceDto>>(performances);
        }

        public async Task<List<SeatDto>> GetAvailableSeatsAsync(int performanceId, int scheduleId, string location)
        {
            var seats = await _unitOfWork.Seats.FindAsync(
                s => s.PerformanceScheduleId == scheduleId &&
                     s.PerformanceSchedule.PerformanceId == performanceId &&
                     s.Location == location &&
                     (s.Ticket == null || s.Ticket.Status != TicketSystem.DAL.TicketStatus.Sold),
                s => s.Ticket,
                s => s.PerformanceSchedule);

            return _mapper.Map<List<SeatDto>>(seats);
        }

        public async Task<int> GetAvailableSeatsCountAsync(int performanceId, int scheduleId, string location)
        {
            var seats = await _unitOfWork.Seats.FindAsync(
                s => s.PerformanceScheduleId == scheduleId &&
                     s.PerformanceSchedule.PerformanceId == performanceId &&
                     s.Location == location &&
                     (s.Ticket == null || s.Ticket.Status != TicketSystem.DAL.TicketStatus.Sold),
                s => s.Ticket,
                s => s.PerformanceSchedule);

            return seats.Count();
        }

        public async Task<TicketDto?> BuyTicketAsync(int performanceId, int scheduleId, int seatId, ITicketPricingStrategy pricingStrategy, string phoneNumber)
        {
            PhoneNumberValidator.Validate(phoneNumber);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var schedule = await _unitOfWork.PerformanceSchedules.FirstOrDefaultAsync(
                    s => s.Id == scheduleId && s.PerformanceId == performanceId);
                if (schedule == null)
                    return null;

                var performance = await _unitOfWork.Performances.FirstOrDefaultAsync(
                    p => p.Id == performanceId,
                    query => query.Include(p => p.Tickets).ThenInclude(t => t.Seat));

                if (performance == null)
                    return null;

                var seat = await _unitOfWork.Seats.FirstOrDefaultAsync(
                    s => s.Id == seatId &&
                         s.PerformanceScheduleId == scheduleId &&
                         s.PerformanceSchedule.PerformanceId == performanceId,
                    query => query.Include(s => s.Ticket));

                if (seat == null ||
                    (seat.Ticket != null &&
                     seat.Ticket.PerformanceId == performanceId &&
                     seat.Ticket.PerformanceScheduleId == scheduleId &&
                     seat.Ticket.Status == TicketSystem.DAL.TicketStatus.Sold))
                    return null;

                var ticket = new Ticket
                {
                    PerformanceId = performanceId,
                    PerformanceScheduleId = scheduleId,
                    SeatId = seatId,
                    Status = TicketSystem.DAL.TicketStatus.Sold,
                    Price = pricingStrategy.CalculatePrice(),
                    PurchaseDate = DateTime.Now,
                    PhoneNumber = phoneNumber
                };

                await _unitOfWork.Tickets.AddAsync(ticket);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<TicketDto>(ticket);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<(bool Success, TicketDto? Ticket)> ReturnTicketAsync(int ticketId, string phoneNumber)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var ticket = await _unitOfWork.Tickets.FirstOrDefaultAsync(
                    t => t.Id == ticketId && t.PhoneNumber == phoneNumber,
                    query => query.Include(t => t.Performance),
                    query => query.Include(t => t.PerformanceSchedule));

                if (ticket == null || ticket.IsReturned || ticket.Status != TicketSystem.DAL.TicketStatus.Sold)
                    return (false, null);

                if ((ticket.PerformanceSchedule.Date - DateTime.Now).TotalDays <= 2)
                    return (false, null);

                ticket.Status = TicketSystem.DAL.TicketStatus.Returned;
                ticket.IsReturned = true;
                ticket.Price *= 0.8m;

                _unitOfWork.Tickets.Update(ticket);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (true, _mapper.Map<TicketDto>(ticket));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<TicketDto?> GetTicketByIdAsync(int id)
        {
            var ticket = await _unitOfWork.Tickets.FirstOrDefaultAsync(
                t => t.Id == id,
                query => query.Include(t => t.Performance),
                query => query.Include(t => t.PerformanceSchedule),
                query => query.Include(t => t.Seat));

            return ticket != null ? _mapper.Map<TicketDto>(ticket) : null;
        }

        public async Task<List<TicketDto>> GetTicketsByStatusAsync(TicketSystem.BLL.Dto.TicketStatus status)
        {
            var dalStatus = status switch
            {
                TicketSystem.BLL.Dto.TicketStatus.Available => TicketSystem.DAL.TicketStatus.Available,
                TicketSystem.BLL.Dto.TicketStatus.Sold => TicketSystem.DAL.TicketStatus.Sold,
                TicketSystem.BLL.Dto.TicketStatus.Returned => TicketSystem.DAL.TicketStatus.Returned,
                _ => throw new ArgumentException($"Unknown TicketStatus: {status}")
            };

            var tickets = await _unitOfWork.Tickets.FindAsync(
                t => t.Status == dalStatus,
                t => t.Performance,
                t => t.Seat);

            return _mapper.Map<List<TicketDto>>(tickets);
        }

    }

    public static class PhoneNumberValidator
    {
        public static void Validate(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length != 10 || !phoneNumber.All(char.IsDigit))
            {
                throw new ArgumentException("Некоректний номер телефону. Формат: 0671234567");
            }
        }
    }

}