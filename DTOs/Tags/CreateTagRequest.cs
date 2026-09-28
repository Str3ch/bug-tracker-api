using System.ComponentModel.DataAnnotations;

namespace BugTracker.Api.DTOs.Tags
{
    public class CreateTagRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;   
    }
}
