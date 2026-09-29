using BugTracker.Api.Enums;

namespace BugTracker.Api.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public AuditAction Action { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string? Details { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    }
}
