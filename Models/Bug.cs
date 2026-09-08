using BugTracker.Api.Enums;
using System.Numerics;

namespace BugTracker.Api.Models
{
    public class Bug
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public BugStatus Status { get; set; }
        public BugPriority Priority { get; set; }
        public BugSeverity Severity { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
