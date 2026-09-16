using BugTracker.Api.Data;
using BugTracker.Api.DTOs.Bugs;
using BugTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace BugTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BugsController : ControllerBase
    {
        private readonly BugTrackerDbContext _context;

        public BugsController(BugTrackerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BugResponse>>> GetBugs()
        {
            var bugs = await _context.Bugs
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
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync();

            return Ok(bugs);
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
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync();

            if (bug == null)
            {
                return NotFound();
            }

            return Ok(bug);
        }
        [HttpPost]
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

            var bug = new Bug
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Priority = request.Priority,
                Severity = request.Severity,
                ProjectId = request.ProjectId
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
                    UpdatedAt = b.UpdatedAt
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetBugs),
                new { id = bug.Id },
                response);
        }

        [HttpPut("{id:int}")]
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
            bug.Status = request.Status;
            bug.Priority = request.Priority;
            bug.Severity = request.Severity;
            bug.ProjectId = request.ProjectId;
            bug.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
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
    }
}

