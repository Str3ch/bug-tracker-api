using BugTracker.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BugTracker.Api.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration) 
        { 
            _configuration = configuration;
        }

        public string GenerateToken(User user) 
        {
            var keyValue = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT key is not configured.");
            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is not configured.");
            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Username.ToString()),
                new Claim(ClaimTypes.Role,user.Role.ToString()),
            };
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
