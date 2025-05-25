using TicketSystem.DAL.Models;
using TicketSystem.DAL.Repositories;

namespace TicketSystem.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Author> Authors { get; }
        IGenericRepository<Genre> Genres { get; }
        IGenericRepository<Performance> Performances { get; }
        IGenericRepository<PerformanceSchedule> PerformanceSchedules { get; }
        IGenericRepository<Seat> Seats { get; }
        IGenericRepository<Ticket> Tickets { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
