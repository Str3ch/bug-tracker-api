namespace BugTracker.Api.Models
{
    public class BugComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int BugId { get; set; }
        public Bug Bug { get; set; } = null!;
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;
    }
}
