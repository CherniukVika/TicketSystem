namespace TicketSystem.PL.Models
{
    public class TicketPLDto
    {
        public int Id { get; set; }
        public int PerformanceId { get; set; }
        public int PerformanceScheduleId { get; set; }
        public int SeatId { get; set; }
        public string SeatLocation { get; set; }
        public decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
    }
}