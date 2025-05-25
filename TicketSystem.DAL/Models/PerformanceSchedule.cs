namespace TicketSystem.DAL.Models
{
    public class PerformanceSchedule
    {
        public int Id { get; set; }
        public int PerformanceId { get; set; }
        public virtual Performance Performance { get; set; } 
        public DateTime Date { get; set; }
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>(); 
    }
}