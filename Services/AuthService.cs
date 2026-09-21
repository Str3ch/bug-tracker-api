using BugTracker.Api.Data;
using BugTracker.Api.DTOs.Auth;
using BugTracker.Api.Enums;
using BugTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Api.Services
{
    public class AuthService
    {
        private readonly BugTrackerDbContext _context;

        private readonly JwtService _jwtService;

        public AuthService(BugTrackerDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var username = request.Username.Trim();
            var usernameExists = await _context.Users.AnyAsync(u => u.Username == username);

            if (usernameExists) return null;

            var user = new User
            {
                Username = username,
                PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Tester
            };
            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role.ToString(),
                Token = _jwtService.GenerateToken(user)
            };
        }
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var username = request.Username.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u  => u.Username == username);

            if (user == null) return null;
            var passwordIsValid = BCrypt.Net.BCrypt.Verify(request.Password,user.PasswordHash);
            if (!passwordIsValid) return null;

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role.ToString(),
                Token = _jwtService.GenerateToken(user)
            };
        }
    }
}
