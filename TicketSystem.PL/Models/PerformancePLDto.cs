using TicketSystem.BLL.Dto;

namespace TicketSystem.PL.Models
{
    public class PerformancePLDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public List<string> Genres { get; set; }
        public List<PerformanceSchedulePLDto> Schedules { get; set; }
    }
}