namespace TicketSystem.BLL.Dto
{
    public class TicketDto
    {
        public int Id { get; set; }
        public bool IsReturned { get; set; }
        public int PerformanceId { get; set; }
        public int SeatId { get; set; }
        public TicketStatus Status { get; set; } 
        public decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PhoneNumber { get; set; }
        public int PerformanceScheduleId { get; set; }
    }
}