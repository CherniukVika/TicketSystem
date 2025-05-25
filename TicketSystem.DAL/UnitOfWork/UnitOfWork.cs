using TicketSystem.DAL.Models;
using TicketSystem.DAL.Repositories;

namespace TicketSystem.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TicketSystemContext _context;
        private bool _disposed = false;

        public IGenericRepository<Author> Authors { get; private set; }
        public IGenericRepository<Genre> Genres { get; private set; }
        public IGenericRepository<Performance> Performances { get; private set; }
        public IGenericRepository<PerformanceSchedule> PerformanceSchedules { get; private set; }
        public IGenericRepository<Seat> Seats { get; private set; }
        public IGenericRepository<Ticket> Tickets { get; private set; }

        public UnitOfWork(TicketSystemContext context)
        {
            _context = context;
            Authors = new GenericRepository<Author>(context);
            Genres = new GenericRepository<Genre>(context);
            Performances = new GenericRepository<Performance>(context);
            PerformanceSchedules = new GenericRepository<PerformanceSchedule>(context);
            Seats = new GenericRepository<Seat>(context);
            Tickets = new GenericRepository<Ticket>(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
