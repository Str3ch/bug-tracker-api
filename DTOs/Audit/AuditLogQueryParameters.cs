using BugTracker.Api.Enums;

namespace BugTracker.Api.DTOs.Audit
{
    public class AuditLogQueryParameters
    {
        public AuditAction? Action { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public int? UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
