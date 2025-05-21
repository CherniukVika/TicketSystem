namespace TicketSystem.Data
{
    public class Seat
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Location { get; set; }
        public int PerformanceScheduleId { get; set; }
        public virtual PerformanceSchedule PerformanceSchedule { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}