namespace TicketSystem.BLL.Dto
{
    public class SeatDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Location { get; set; }
        public int PerformanceScheduleId { get; set; }
        public TicketDto? Ticket { get; set; }
    }
}
