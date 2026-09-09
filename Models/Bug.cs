using BugTracker.Api.Enums;
using System.Numerics;

namespace BugTracker.Api.Models
{
    public class Bug
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public BugStatus Status { get; set; } = BugStatus.Open;
        public BugPriority Priority { get; set; } = BugPriority.Medium;
        public BugSeverity Severity { get; set; } = BugSeverity.Major;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
    }
}
