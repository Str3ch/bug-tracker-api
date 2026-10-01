using BugTracker.Api.Data;
using BugTracker.Api.Enums;
using BugTracker.Api.Models;
using System.Security.Claims;

namespace BugTracker.Api.Services
{
    public class AuditService
    {
        private readonly BugTrackerDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(BugTrackerDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public void Add(AuditAction action, string entityType,
            int entityId, string? details = null)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new InvalidOperationException(
                    "Authenticated user id was not found");
            }

            var auditLog = new AuditLog
            {
                Action = action,
                EntityId = entityId,
                UserId = userId,
                Details = details,
                CreatedAt = DateTime.UtcNow,
            };

            _context.AuditLogs.Add(auditLog);
        }
    }
}
