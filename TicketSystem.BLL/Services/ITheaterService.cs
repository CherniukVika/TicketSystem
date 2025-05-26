using TicketSystem.BLL.Dto;
using TicketSystem.BLL.Strategy;

namespace TicketSystem.BLL.Services
{
    public interface ITheaterService
    {
        Task<string> GetAuthorNameAsync(int performanceId);
        Task<List<string>> GetGenreNamesAsync(int performanceId);
        Task<List<PerformanceDto>> GetAllPerformancesAsync();
        Task<List<SeatDto>> GetAvailableSeatsAsync(int performanceId, int scheduleId, string location);
        Task<int> GetAvailableSeatsCountAsync(int performanceId, int scheduleId, string location);
        Task<TicketDto?> BuyTicketAsync(int performanceId, int scheduleId, int seatId, ITicketPricingStrategy pricingStrategy, string phoneNumber);
        Task<(bool Success, TicketDto? Ticket)> ReturnTicketAsync(int ticketId, string phoneNumber);
        Task<List<TicketDto>> GetTicketsByStatusAsync(TicketSystem.BLL.Dto.TicketStatus status);
    }
}