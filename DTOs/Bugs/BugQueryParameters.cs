using BugTracker.Api.Enums;
using System.Numerics;

namespace BugTracker.Api.DTOs.Bugs
{
    public class BugQueryParameters
    {
        public string? Search { get; set; } 
        public BugStatus? Status { get; set; }
        public BugPriority? Priority { get; set; }
        public BugSeverity? Severity { get; set; }
        public int? ProjectId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
