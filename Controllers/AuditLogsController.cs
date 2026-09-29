using BugTracker.Api.Data;
using BugTracker.Api.DTOs;
using BugTracker.Api.DTOs.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly BugTrackerDbContext _context;

        public AuditLogsController(BugTrackerDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<PagedResult<AuditLogResponse>>>
            GetAuditLogs([FromQuery] AuditLogQueryParameters parameters)
        {
            if (parameters.Page < 1) parameters.Page = 1;
            if (parameters.PageSize < 1) parameters.PageSize = 50;
            if (parameters.PageSize > 100) parameters.PageSize = 100;
            var query = _context.AuditLogs.AsQueryable();
            if (parameters.Action.HasValue)
            {
                query = query.Where(a => a.Action == parameters.Action.Value); 
            }
            if (!string.IsNullOrWhiteSpace(parameters.EntityType))
            {
                var entityType = parameters.EntityType.Trim();
                query = query.Where(a => a.EntityType == entityType);
            }
            if (parameters.EntityId.HasValue)
            {
                query = query.Where(a => a.EntityId == parameters.EntityId.Value);
            }
            if (parameters.UserId.HasValue)
            {
                query = query.Where(a => a.UserId == parameters.UserId.Value);
            }

            var totalCount = await query.CountAsync();

            var logs = await query.OrderByDescending(a => a.CreatedAt)
                .Skip((parameters.Page - 1) *
                parameters.PageSize).Take(parameters.PageSize)
                .Select(a => new AuditLogResponse
                {
                    Id = a.Id,
                    Action = a.Action.ToString(),
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    UserId = a.UserId,
                    Username = a.User.Username,
                    CreatedAt = a.CreatedAt
                }).ToListAsync();

            var result = new PagedResult<AuditLogResponse>
            {
                Items = logs,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
            };

            return Ok(result);
        }
    }
}
