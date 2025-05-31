namespace TicketSystem.PL.Models
{
    public class BuyTicketRequestDto
    {
        public int PerformanceId { get; set; } 
        public int ScheduleId { get; set; }
        public int SeatId { get; set; }
        public string Location { get; set; }
        public string PhoneNumber { get; set; }
    }
}