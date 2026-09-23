using BugTracker.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Api.DTOs.Bugs
{
    public class UpdateBugRequest
    {
        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        public BugPriority Priority { get; set; }

        public BugSeverity Severity { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }
    }
}
