namespace BugTracker.Api.DTOs.Dashboard
{
    public class DeveloperWorkloadResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int AssignedBugs { get; set; }
        public int InProgressBugs { get; set; }
    }
}
