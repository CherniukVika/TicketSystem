namespace TicketSystem.DAL.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Performance> Performances { get; set; } = new List<Performance>();
    }
}
