using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;
using TicketSystem.ConsoleApp.Strategy;

namespace TicketSystem.ConsoleApp
{
    public class TheaterService
    {
        private readonly TicketSystemContext _context;

        public TheaterService(TicketSystemContext context)
        {
            _context = context;
        }

        public string GetAuthorName(int performanceId)
        {
            var performance = _context.Performances
                .Include(p => p.Author)
                .FirstOrDefault(p => p.Id == performanceId);

            return performance?.Author?.Name ?? "Невідомий автор";
        }

        public List<string> GetGenreNames(int performanceId)
        {
            var performance = _context.Performances
                .Include(p => p.Genres)
                .FirstOrDefault(p => p.Id == performanceId);

            return performance?.Genres.Select(g => g.Name).ToList() ?? new List<string>();
        }

        public List<Performance> GetAllPerformances()
        {
            return _context.Performances
                .Include(p => p.Author)
                .Include(p => p.Genres)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.Seat)
                .Include(p => p.Schedules)
                .ToList();
        }

        public List<Seat> GetAvailableSeats(int performanceId, int scheduleId, string location)
        {
            return _context.Seats
                .Where(s => s.PerformanceScheduleId == scheduleId &&
                            s.PerformanceSchedule.PerformanceId == performanceId &&
                            s.Location == location &&
                            (s.Ticket == null || s.Ticket.Status != TicketStatus.Sold))
                .ToList();
        }

        public int GetAvailableSeatsCount(int performanceId, int scheduleId, string location)
        {
            return _context.Seats
                .Where(s => s.PerformanceScheduleId == scheduleId &&
                            s.PerformanceSchedule.PerformanceId == performanceId &&
                            s.Location == location &&
                            (s.Ticket == null || s.Ticket.Status != TicketStatus.Sold))
                .Count();
        }

        public Ticket? BuyTicket(int performanceId, int scheduleId, int seatId, ITicketPricingStrategy pricingStrategy, string phoneNumber)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var performance = _context.Performances
                         .Include(p => p.Tickets)
                             .ThenInclude(t => t.Seat)
                         .FirstOrDefault(p => p.Id == performanceId);

                    if (performance == null) return null;

                    var seat = _context.Seats
                        .Include(s => s.Ticket)
                        .FirstOrDefault(s => s.Id == seatId &&
                                           s.PerformanceScheduleId == scheduleId &&
                                           s.PerformanceSchedule.PerformanceId == performanceId);

                    if (seat == null ||
                        (seat.Ticket != null &&
                         seat.Ticket.PerformanceId == performanceId &&
                         seat.Ticket.PerformanceScheduleId == scheduleId &&
                         seat.Ticket.Status == TicketStatus.Sold))
                        return null;

                    var ticket = new Ticket
                    {
                        PerformanceId = performanceId,
                        PerformanceScheduleId = scheduleId,
                        SeatId = seatId,
                        Status = TicketStatus.Sold,
                        Price = pricingStrategy.CalculatePrice(),
                        PurchaseDate = DateTime.Now,
                        PhoneNumber = phoneNumber
                    };

                    _context.Tickets.Add(ticket);
                    _context.SaveChanges();
                    transaction.Commit();
                    return ticket;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] BuyTicket failed: {ex.Message}");
                    transaction.Rollback();
                    return null;
                }
            }
        }

        public bool ReturnTicket(int ticketId, string phoneNumber)
        {
            var ticket = _context.Tickets
                .Include(t => t.Performance)
                .Include(t => t.PerformanceSchedule)
                .FirstOrDefault(t => t.Id == ticketId && t.PhoneNumber == phoneNumber);

            if (ticket == null || ticket.IsReturned || ticket.Status != TicketStatus.Sold)
                return false;

            if ((ticket.PerformanceSchedule.Date - DateTime.Now).TotalDays <= 2)
                return false;

            ticket.Status = TicketStatus.Returned;
            ticket.IsReturned = true;
            ticket.Price *= 0.8m;
            _context.SaveChanges();
            return true;
        }

        public List<Ticket> GetTicketsByStatus(TicketStatus status)
        {
            return _context.Tickets
                .Where(t => t.Status == status)
                .Include(t => t.Performance)
                .Include(t => t.Seat)
                .ToList();
        }
    }
}