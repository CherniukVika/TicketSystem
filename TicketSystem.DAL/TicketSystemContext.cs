using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using TicketSystem.DAL.Models;

namespace TicketSystem.DAL
{
    public class TicketSystemContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Performance> Performances { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<PerformanceSchedule> PerformanceSchedules { get; set; }

        public TicketSystemContext(DbContextOptions<TicketSystemContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TicketSystem;Trusted_Connection=True;")
                              .UseLazyLoadingProxies();
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1:1 
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithOne(s => s.Ticket)
                .HasForeignKey<Ticket>(t => t.SeatId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:M 
            modelBuilder.Entity<Performance>()
                .HasMany(p => p.Schedules)
                .WithOne(s => s.Performance)
                .HasForeignKey(s => s.PerformanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M 
            modelBuilder.Entity<Author>()
                .HasMany(a => a.Performances)
                .WithOne(p => p.Author)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M 
            modelBuilder.Entity<Performance>()
                .HasMany(p => p.Tickets)
                .WithOne(t => t.Performance)
                .HasForeignKey(t => t.PerformanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M 
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.PerformanceSchedule)
                .WithMany()
                .HasForeignKey(t => t.PerformanceScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M 
            modelBuilder.Entity<PerformanceSchedule>()
                .HasMany(ps => ps.Seats)
                .WithOne(s => s.PerformanceSchedule)
                .HasForeignKey(s => s.PerformanceScheduleId);

            // M:N 
            modelBuilder.Entity<Performance>()
                .HasMany(p => p.Genres)
                .WithMany(g => g.Performances)
                .UsingEntity(j => j.ToTable("PerformanceGenres"));

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);
        }
    }

    public class TicketSystemContextFactory : IDesignTimeDbContextFactory<TicketSystemContext>
    {
        public TicketSystemContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TicketSystemContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TicketSystem;Trusted_Connection=True;");
            return new TicketSystemContext(optionsBuilder.Options);
        }
    }
}
