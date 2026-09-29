using BugTracker.Api.Data;
using BugTracker.Api.DTOs.Tags;
using BugTracker.Api.Enums;
using BugTracker.Api.Models;
using BugTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly BugTrackerDbContext _context;
        private readonly AuditService _auditService;
        public TagsController(BugTrackerDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagResponse>>> GetTags()
        {
            var tags = await _context.Tags
                .OrderBy(t => t.Name)
                .Select(t => new TagResponse
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToListAsync();
            return Ok(tags);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TagResponse>> CreateTag(CreateTagRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new
                {
                    message = "Tag name cannot be empty"
                });
            }
            var name = request.Name.Trim().ToLowerInvariant();

            var exists = await _context.Tags.AnyAsync(t => t.Name == name);

            if (exists)
            {
                return Conflict(new
                {
                    message = "Tag already exists"
                });
            }
            var tag = new Tag
            {
                Name = name
            };

            _context.Tags.Add(tag);

            await _context.SaveChangesAsync();

            _auditService.Add(AuditAction.TagCreated,
                "Tag",tag.Id, $"Tag '{tag.Name}' was created.");

            await _context.SaveChangesAsync();

            var response = new TagResponse
            {
                Id = tag.Id,
                Name = tag.Name
            };
            return StatusCode(StatusCodes.Status201Created, response);
        }
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tags.FindAsync(id);

            if (tag == null)
            {
                return NotFound(new
                {
                    message = "Tag does not exist"
                });
            }

            var isUsed = await _context.BugTags.IgnoreQueryFilters()
                .AnyAsync(bt => bt.TagId == id);

            if (isUsed)
            {
                return Conflict(new
                {
                    message = "Tag cannot be deleted while it is assigned to bugs"
                });
            }
            _auditService.Add(AuditAction.TagDeleted,
                "Tag",tag.Id,$"Tag '{tag.Name}' was deleted.");

            _context.Tags.Remove(tag);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
