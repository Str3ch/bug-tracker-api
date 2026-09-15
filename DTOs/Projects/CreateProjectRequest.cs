using System.ComponentModel.DataAnnotations;

namespace BugTracker.Api.DTOs.Projects
{
    public class CreateProjectRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
