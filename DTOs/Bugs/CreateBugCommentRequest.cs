using System.ComponentModel.DataAnnotations;

namespace BugTracker.Api.DTOs.Bugs
{
    public class CreateBugCommentRequest
    {
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}
