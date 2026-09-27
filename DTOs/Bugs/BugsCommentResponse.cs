namespace BugTracker.Api.DTOs.Bugs
{
    public class BugsCommentResponse
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
