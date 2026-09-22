using BugTracker.Api.Data;
using BugTracker.Api.DTOs.Projects;
using BugTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly BugTrackerDbContext _context;
        public ProjectsController(BugTrackerDbContext context)
        {
            _context = context;
        }

        //GET:  api/projects

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
        {
            var projects = await _context.Projects
                .Select(p => new ProjectResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    BugsCount = p.Bugs.Count

                })
                .ToListAsync();
            return Ok(projects);
        }

        //GET api/projects/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProjectResponse>> GetProject(int id)
        {
            var project = await _context.Projects
                .Where(p => p.Id == id)
                .Select(p => new ProjectResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    BugsCount = p.Bugs.Count
                })
                .FirstOrDefaultAsync();
            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }
       
        
        //POST api/projects
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProjectResponse>> CreateProject(
            CreateProjectRequest request)
        {
            var project = new Project
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim()
            };
            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            var response = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                BugsCount = 0
            };
            return CreatedAtAction
                (
                nameof(GetProject),
                new { id = project.Id },
                response);
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProject(int id,UpdateProjectRequest request)
        {
            var project = await _context.Projects
                .FindAsync(id);
            if (project == null) { return NotFound(); }  
            project.Name = request.Name.Trim();
            project.Description = request.Description?.Trim();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        //DELETE: api/projects/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects
                .FindAsync(id);

            if (project == null) return NotFound();

            bool ContainsBugs = await _context.Bugs
                .AnyAsync(b => b.ProjectId == id);

            if (ContainsBugs)
            {
                return Conflict(new
                {
                    message = "Project containing bugs cannot be deleted"
                });
            }
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();
            
            return NoContent();
        }

    }
}
