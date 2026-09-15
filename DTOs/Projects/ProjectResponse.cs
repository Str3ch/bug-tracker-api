namespace BugTracker.Api.DTOs.Projects
{
    public class ProjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BugsCount { get; set; }
    }
}
