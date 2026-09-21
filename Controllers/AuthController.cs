using BugTracker.Api.DTOs.Auth;
using BugTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace BugTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result == null)
            {
                return Conflict(new
                {
                    message = "Username already exists"
                });
            }
            return StatusCode(StatusCodes.Status201Created, result);
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid login or password"
                });
            }
            return Ok(result);
        }
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                username = User.FindFirstValue(ClaimTypes.Name),
                role = User.FindFirstValue(ClaimTypes.Role)
            });
        }
    }
}
