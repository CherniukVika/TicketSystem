namespace TicketSystem.DAL.Models
{
    public class Performance
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int AuthorId { get; set; }
        public virtual Author Author { get; set; } 
        public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>(); 
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>(); 
        public virtual ICollection<PerformanceSchedule> Schedules { get; set; } = new List<PerformanceSchedule>(); 
    }
}