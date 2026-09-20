namespace BugTracker.Api.DTOs.Bugs
{
    public class BugResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int? CreatedById { get; set;}
        public string? CreatedByUsername { get; set; }
        public int? AssignedToId { get; set; }
        public string? AssignedToUsername { get; set; }
    }
}
