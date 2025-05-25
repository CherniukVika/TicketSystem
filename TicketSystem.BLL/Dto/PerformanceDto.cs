namespace TicketSystem.BLL.Dto
{
    public class PerformanceDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public AuthorDto Author { get; set; }
        public List<GenreDto> Genres { get; set; }
        public List<TicketDto> Tickets { get; set; } 
        public List<PerformanceScheduleDto> Schedules { get; set; }
    }
}
