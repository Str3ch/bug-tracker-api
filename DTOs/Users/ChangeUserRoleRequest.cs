using BugTracker.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Api.DTOs.Users
{
    public class ChangeUserRoleRequest
    {
        [Required]
        public UserRole Role { get; set; }
    }
}
