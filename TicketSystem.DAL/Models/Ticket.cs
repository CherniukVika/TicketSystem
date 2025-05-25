namespace TicketSystem.DAL.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public bool IsReturned { get; set; }
        public int PerformanceId { get; set; }
        public virtual Performance Performance { get; set; } 
        public int SeatId { get; set; }
        public virtual Seat Seat { get; set; } 
        public TicketStatus Status { get; set; }
        public decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PhoneNumber { get; set; }
        public int PerformanceScheduleId { get; set; }
        public virtual PerformanceSchedule PerformanceSchedule { get; set; } 
    }
}