namespace TicketSystem.BLL.Dto
{
    public class PerformanceScheduleDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int PerformanceId { get; set; }
        public List<SeatDto> Seats { get; set; }
        public int AvailableSeats { get; set; }
    }
}
