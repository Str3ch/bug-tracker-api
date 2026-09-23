using BugTracker.Api.Data;
using BugTracker.Api.DTOs;
using BugTracker.Api.DTOs.Bugs;
using BugTracker.Api.Models;
using BugTracker.Api.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace BugTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BugsController : ControllerBase
    {
        private readonly BugTrackerDbContext _context;

        public BugsController(BugTrackerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BugResponse>>> GetBugs(
            [FromQuery] BugQueryParameters parameters)
        {
            if (parameters.Page < 1) parameters.Page = 1;
            if (parameters.PageSize < 1) parameters.PageSize = 20;
            if (parameters.PageSize > 100) parameters.PageSize = 100;

            var query = _context.Bugs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.Trim();

                query = query.Where(b =>
                b.Title.Contains(search) ||
                b.Description.Contains(search));
            }
            if (parameters.Status.HasValue)
            {
                query = query.Where(b =>
                    b.Status == parameters.Status.Value);
            }

            if (parameters.Priority.HasValue)
            {
                query = query.Where(b =>
                    b.Priority == parameters.Priority.Value);
            }

            if (parameters.Severity.HasValue)
            {
                query = query.Where(b =>
                    b.Severity == parameters.Severity.Value);
            }

            if (parameters.ProjectId.HasValue)
            {
                query = query.Where(b =>
                    b.ProjectId == parameters.ProjectId.Value);
            }

            var totalCount = await query.CountAsync();

            var bugs = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(b => new BugResponse
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Status = b.Status.ToString(),
                    Priority = b.Priority.ToString(),
                    Severity = b.Severity.ToString(),
                    ProjectId = b.ProjectId,
                    ProjectName = b.Project.Name,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,

                    CreatedById = b.CreatedById,
                    CreatedByUsername = b.CreatedBy != null
                    ? b.CreatedBy.Username : null,

                    AssignedToId = b.AssignedToId,
                    AssignedToUsername = b.AssignedTo != null
                    ? b.AssignedTo.Username : null,
                })
                .ToListAsync();

            var result = new PagedResult<BugResponse>
            {
                Items = bugs,
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)parameters.PageSize)
            };

            return Ok(result);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BugResponse>> GetBugs(int id)
        {
            var bug = await _context.Bugs
            .Where(b => b.Id == id)
            .Select(b => new BugResponse
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                Status = b.Status.ToString(),
                Priority = b.Priority.ToString(),
                Severity = b.Severity.ToString(),
                ProjectId = b.ProjectId,
                ProjectName = b.Project.Name,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,

                CreatedById = b.CreatedById,
                CreatedByUsername = b.CreatedBy != null
                    ? b.CreatedBy.Username : null,

                AssignedToId = b.AssignedToId,
                AssignedToUsername = b.AssignedTo != null
                    ? b.AssignedTo.Username : null,
            })
            .FirstOrDefaultAsync();

            if (bug == null)
            {
                return NotFound();
            }

            return Ok(bug);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Tester")]
        public async Task<ActionResult<BugResponse>> CreateBug(
        CreateBugRequest request)
        {
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == request.ProjectId);

            if (!projectExists)
            {
                return BadRequest(new
                {
                    message = "Project does not exist."
                });
            }
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var bug = new Bug
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Priority = request.Priority,
                Severity = request.Severity,
                ProjectId = request.ProjectId,
                CreatedById = userId
            };

            _context.Bugs.Add(bug);

            await _context.SaveChangesAsync();

            var response = await _context.Bugs
                .Where(b => b.Id == bug.Id)
                .Select(b => new BugResponse
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Status = b.Status.ToString(),
                    Priority = b.Priority.ToString(),
                    Severity = b.Severity.ToString(),
                    ProjectId = b.ProjectId,
                    ProjectName = b.Project.Name,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,

                    CreatedById = b.CreatedById,
                    CreatedByUsername = b.CreatedBy != null
                    ? b.CreatedBy.Username : null,

                    AssignedToId = b.AssignedToId,
                    AssignedToUsername = b.AssignedTo != null
                    ? b.AssignedTo.Username : null,
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetBugs),
                new { id = bug.Id },
                response);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Tester")]
        public async Task<IActionResult> UpdateBug(
            int id,
            UpdateBugRequest request)
        {
            var bug = await _context.Bugs
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bug == null)
            {
                return NotFound();
            }

            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == request.ProjectId);

            if (!projectExists)
            {
                return BadRequest(new
                {
                    message = "Project does not exist."
                });
            }

            bug.Title = request.Title.Trim();
            bug.Description = request.Description.Trim();
            bug.Priority = request.Priority;
            bug.Severity = request.Severity;
            bug.ProjectId = request.ProjectId;
            bug.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBug(int id)
        {
            var bug = await _context.Bugs
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bug == null)
            {
                return NotFound();
            }

            bug.IsDeleted = true;
            bug.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPut("{id:int}/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignBug(int id, AssignedBugRequest request)
        {
            var bug = await _context.Bugs.FirstOrDefaultAsync(b => b.Id == id);
            if (bug == null)
            {
                return NotFound(new
                {
                    message = "Bug does not exist"
                });
            }
            if (request.UserId == null)
            {
                bug.AssignedToId = null;
                bug.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId.Value);
            if (user == null)
            {
                return NotFound(new
                {
                    message = "User does not exist"
                });

            }
            if (user.Role != UserRole.Developer)
            {
                return BadRequest(new
                {
                    message = "Bug can only be assigned to a developer"
                });
            }

            bug.AssignedToId = user.Id;
            bug.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Admin,Developer,Tester")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeBugStatusRequest request)
        {
            if (!Enum.IsDefined(request.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid bug status"
                });
            }
            var bug = await _context.Bugs.FirstOrDefaultAsync(b => b.Id == id);
            if (bug == null)
            {
                return NotFound(new
                {
                    message = "Bug does not exist"
                });
            }
            if (bug.Status == request.Status)
            {
                return BadRequest(new
                {
                    message = "Bug already has this status."
                });
            }
            if (!IsValidStatusTransition(bug.Status, request.Status))
            {
                return BadRequest(new
                {
                    message = $"Status cannot be changed from {bug.Status} to {request.Status}"
                });
            }
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();
            var isAdmin = User.IsInRole("Admin");
            var isDeveloper = User.IsInRole("Developer");
            var isTester = User.IsInRole("Tester");

            if (isDeveloper)
            {
                if (bug.AssignedToId != userId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden
                        , new
                        {
                            message = "Developer can only change status of bugs assigned to them"
                        });
                }
                if (!CanDeveloperPerformTransition(bug.Status, request.Status))
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new
                        {
                            message = "Developer cannot perform this status transition"
                        });
                }
            }
            if (isTester && !CanTesterPerformTransition(bug.Status, request.Status))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Tester cannot perform this status transition"
                });
            }
            if (!isAdmin && !isDeveloper && !isTester) return Forbid();

            var oldStatus = bug.Status;

            bug.Status = request.Status;
            bug.UpdatedAt = DateTime.UtcNow;

            var history = new BugStatusHistory
            {
                BugId = bug.Id,
                OldStatus = oldStatus,
                NewStatus = request.Status,
                ChangedById = userId
            };
            _context.BugStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("{id:int}/status-history")]
        public async Task<ActionResult<IEnumerable<BugStatusHistoryResponse>>>
        GetStatusHistory(int id)
        {
            var bugExists = await _context.Bugs.AnyAsync(b => b.Id == id);
            if (!bugExists)
            {
                return NotFound(new
                {
                    message = "Bug does not exist"
                });
            }
            var history = await _context.BugStatusHistories
            .Where(h => h.BugId == id)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new BugStatusHistoryResponse
            {
                Id = h.Id,

                OldStatus = h.OldStatus.ToString(),

                NewStatus = h.NewStatus.ToString(),

                ChangedById = h.ChangedById,

                ChangedByUsername = h.ChangedBy.Username,

                ChangedAt = h.ChangedAt
            }).ToListAsync();
            return Ok(history);
        }
        private static bool IsValidStatusTransition(BugStatus oldStatus, BugStatus newStatus)
        {
            return (oldStatus, newStatus) switch
            {
                (BugStatus.Open, BugStatus.InProgress) => true,
                (BugStatus.InProgress, BugStatus.Resolved) => true,
                (BugStatus.Resolved, BugStatus.Closed) => true,
                (BugStatus.Resolved, BugStatus.Reopened) => true,
                (BugStatus.Closed, BugStatus.Reopened) => true,
                (BugStatus.Reopened, BugStatus.InProgress) => true,
                _ => false
            };
        }
        private static bool CanDeveloperPerformTransition(BugStatus oldStatus, BugStatus newStatus)
        {
            return (oldStatus, newStatus) switch
            {
                (BugStatus.Open, BugStatus.InProgress) => true,
                (BugStatus.InProgress, BugStatus.Resolved) => true,
                (BugStatus.Reopened, BugStatus.InProgress) => true,
                _ => false
            };
        }
        private static bool CanTesterPerformTransition(BugStatus oldStatus, BugStatus newStatus)
        {
            return (oldStatus, newStatus) switch
            {
                (BugStatus.Resolved, BugStatus.Closed) => true,
                (BugStatus.Resolved, BugStatus.Reopened) => true,
                (BugStatus.Closed, BugStatus.Reopened) => true,
                _ => false
            };
        }
    }
}
