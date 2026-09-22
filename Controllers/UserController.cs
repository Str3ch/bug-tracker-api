using BugTracker.Api.Data;
using BugTracker.Api.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly BugTrackerDbContext _context;

        public UserController(BugTrackerDbContext context) 
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.OrderBy(u => u.Username)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    Role = u.Role.ToString(),
                    u.CreatedAt
                }).ToListAsync();
            return Ok(users);
        }
        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> ChangeRole(int id, ChangeUserRoleRequest request)
        {
            if (!Enum.IsDefined(request.Role))
            {
                return BadRequest(new
                {
                    message = "Invalid user role."
                });
            } if (!Enum.IsDefined(request.Role))
    {
        return BadRequest(new
        {
            message = "Invalid user role."
        });
    }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new
                {
                    message = "User does not exist"
                });
            }
            user.Role = request.Role;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
