using BugTracker.Api.Enums;

namespace BugTracker.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Tester;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<Bug> CreatedBugs { get; set; } = new List<Bug>();
        public ICollection<Bug> AssignedBugs { get; set; } = new List<Bug>();
    }
}
