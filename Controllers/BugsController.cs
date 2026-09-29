using Azure.Core;
using BugTracker.Api.Data;
using BugTracker.Api.DTOs;
using BugTracker.Api.DTOs.Bugs;
using BugTracker.Api.DTOs.Tags;
using BugTracker.Api.Enums;
using BugTracker.Api.Models;
using BugTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;


namespace BugTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BugsController : ControllerBase
    {
        private readonly BugTrackerDbContext _context;
        private readonly AuditService _auditService;

        public BugsController(BugTrackerDbContext context,
            AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
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
            if (!string.IsNullOrWhiteSpace(parameters.Tag))
            {
                var tag = parameters.Tag.Trim().ToLowerInvariant();
                query = query.Where(b =>
                b.BugTags.Any(bt => bt.Tag.Name == tag));
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

                    Tags = b.BugTags
                    .OrderBy(bt => bt.Tag.Name)
                    .Select(bt => new TagResponse
                    {
                        Id = bt.Tag.Id,
                        Name = bt.Tag.Name,
                    }).ToList(),
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
                Tags = b.BugTags
                    .OrderBy(bt => bt.Tag.Name)
                    .Select(bt => new TagResponse
                    {
                        Id = bt.Tag.Id,
                        Name = bt.Tag.Name,
                    }).ToList(),
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

            _auditService.Add(AuditAction.BugCreated, "Bug", bug.Id,
                $"Big '{bug.Title}' was created");
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
                    Tags = b.BugTags
                    .OrderBy(bt => bt.Tag.Name)
                    .Select(bt => new TagResponse
                    {
                        Id = bt.Tag.Id,
                        Name = bt.Tag.Name,
                    }).ToList(),
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

            _auditService.Add(AuditAction.BugUpdated,
                "Bug", bug.Id,
                "Bug details were updated");

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

            _auditService.Add(AuditAction.BugDeleted,
                "Bug", bug.Id,
                $"Bug '{bug.Title}' was soft deleted");

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
                bug.AssignedTo = null;
                bug.UpdatedAt = DateTime.UtcNow;

                _auditService.Add(AuditAction.BugUnassigned,
                    "Bug", bug.Id, "Developer was unassigned from the bug");
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

            _auditService.Add(AuditAction.BugAasigned,
                "Bug", bug.Id, $"Bug was assigned to '{user.Username}' (UserId = {user.Id})");

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
            _auditService.Add(AuditAction.BugStatusChanged,
                "Bug",bug.Id, $"{oldStatus} -> {request.Status}");
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
        [HttpPost("{id:int}/comment")]
        public async Task<ActionResult<BugsCommentResponse>> AddComment(int id, CreateBugCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new
                {
                    message = "Comment cannot be empty"
                });
            }
            var bugExists = await _context.Bugs.AnyAsync(b => b.Id == id);
            if (!bugExists)
            {
                return NotFound(new
                {
                    message = "Bug does not exist"
                });
            }
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var comment = new BugComment
            {
                BugId = id,
                AuthorId = userId,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
            };

            _context.BugComments.Add(comment);

            _auditService.Add(AuditAction.CommentAdded,
                "Bug", id, "Comment was added");

            await _context.SaveChangesAsync();

            var username = User.FindFirstValue(ClaimTypes.Name)
                ?? string.Empty;

            var response = new BugsCommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorId = userId,
                AuthorUsername = username,
                CreatedAt = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpGet("{id:int}/comments")]
        public async Task<ActionResult<IEnumerable<BugsCommentResponse>>> GetComments(int id)
        {
            var bugExists = await _context.Bugs
                .AnyAsync(b => b.Id == id);

            if (!bugExists)
            {
                return NotFound(new
                {
                    message = "Bug does not exist."
                });
            }

            var comments = await _context.BugComments
                .Where(c => c.BugId == id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new BugsCommentResponse
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorId = c.AuthorId,
                    AuthorUsername = c.Author.Username,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }
        [HttpPost("{id:int}/tags/{tagId:int}")]
        [Authorize(Roles = "Admin,Tester")]
        public async Task<IActionResult> AddTag(int id, int tagId)
        {
            var bugExists = await _context.Bugs.AnyAsync(b => b.Id == id);
            if (!bugExists)
            {
                return NotFound(new
                {
                    message = "Bug does not exist"
                });
            }
            var tag= await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId);

            if (tag == null)
            {
                return NotFound(new
                {
                    message = "Tag does not exist"
                });
            }
            var alreadyAssigned = await _context.BugTags.AnyAsync(bt =>
            bt.BugId == id && bt.TagId == tagId);

            if (alreadyAssigned)
            {
                return Conflict(new
                {
                    message = "Tag is already assigned to this bug"
                });
            }

            var bugTag = new BugTag
            {
                BugId = id,
                TagId = tagId
            };
            _context.BugTags.Add(bugTag);
            _auditService.Add(AuditAction.TagAdded,
                "Bug",id,$"Tag '{tag.Name}' was added");
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id:int}/tags/{tagId:int}")]
        [Authorize(Roles = "Admin,Tester")]
        public async Task<IActionResult> RemoveTag(int id, int tagId)
        {
            var bugExists = await _context.Bugs.AnyAsync(b => b.Id == id);

            if (!bugExists)
            {
                return NotFound(new
                {
                    message = "Bug does not exist"
                });
            }
            var bugTag = await _context.BugTags.Include(bt => bt.Tag)
                .FirstOrDefaultAsync(bt => bt.BugId == id && bt.TagId == tagId);

            if (bugTag == null)
            {
                return NotFound(new
                {
                    message = "Tag is not assigned to this bug"
                });
            }

            var tagName = bugTag.Tag.Name;

            

            _context.BugTags.Remove(bugTag);

            _auditService.Add(AuditAction.TagRemoved,
                "Bug",id,$"Tag '{tagName}' was removed");

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("{id:int}/tags")]
        public async Task<ActionResult<IEnumerable<TagResponse>>> GetBugTags(int id)
        {
            var bugExists = await _context.Bugs.AnyAsync(b => b.Id == id);

            if (!bugExists)
            {
                return NotFound(new
                {
                    message = "Bug does not exist"
                });
            }

            var tags = await _context.BugTags
                .Where(bt => bt.BugId == id)
                .OrderBy(bt => bt.Tag.Name)
                .Select(bt => new TagResponse
                {
                    Id = bt.Tag.Id,
                    Name = bt.Tag.Name,
                }).ToListAsync();

            return Ok(tags);
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
