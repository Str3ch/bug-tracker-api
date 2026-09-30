using BugTracker.Api.Data;
using BugTracker.Api.DTOs.Dashboard;
using BugTracker.Api.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly BugTrackerDbContext _context;

        public DashboardController(BugTrackerDbContext context) 
        { 
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardResponse>> GetDashboard()
        {
            var totalBugs = await _context.Bugs.CountAsync();

            var unassignedBugs = await _context.Bugs.CountAsync(b => b.AssignedToId == null);

            var totalProjects = await _context.Projects.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            var openBugs = await _context.Bugs.CountAsync(b => b.Status == BugStatus.Open);
            var inProgressBugs = await _context.Bugs.CountAsync(b => b.Status == BugStatus.InProgress);
            var resolvedBugs = await _context.Bugs.CountAsync(b => b.Status == BugStatus.Resolved);

            var closedBugs = await _context.Bugs.CountAsync(b => b.Status == BugStatus.Closed);

            var reopenedBugs = await _context.Bugs.CountAsync(b=> b.Status == BugStatus.Reopened);
            var criticalPriorityBugs = await _context.Bugs.CountAsync(b => b.Priority == BugPriority.Critical);
            var blockerSeverityBugs = await _context.Bugs.CountAsync(b => b.Severity == BugSeverity.Blocker);
            var bugsByProject = await _context.Projects.Select(p => new ProjectBugStatsResponse
            {
                ProjectId = p.Id,
                ProjectName = p.Name,
                BugCount = p.Bugs.Count
            }).OrderByDescending(p => p.BugCount).ToListAsync();

            var developerWorkload = await _context.Users
                .Where(u => u.Role == UserRole.Developer)
                .Select(u => new DeveloperWorkloadResponse
                {
                    UserId = u.Id,
                    Username = u.Username,
                    AssignedBugs = u.AssignedBugs.Count(b => b.Status != BugStatus.Closed && b.Status != BugStatus.Resolved),
                    InProgressBugs = u.AssignedBugs.Count(b => b.Status == BugStatus.InProgress)
                }).OrderByDescending(d => d.AssignedBugs).ToListAsync();
            var response = new DashboardResponse
            {
                TotalBugs = totalBugs,
                UnassignedBugs = unassignedBugs,
                TotalProjects = totalProjects,
                TotalUsers = totalUsers,
                OpenBugs = openBugs,
                InProgressBugs = inProgressBugs,
                ResolvedBugs = resolvedBugs,
                ClosedBugs = closedBugs,
                ReopenedBugs = reopenedBugs,
                CriticalPriorityBugs = criticalPriorityBugs,
                BlockerSeverityBugs = blockerSeverityBugs,
                BugsByProject = bugsByProject,
                DeveloperWorkload = developerWorkload
            };

            return Ok(response);

        }
    }
}
