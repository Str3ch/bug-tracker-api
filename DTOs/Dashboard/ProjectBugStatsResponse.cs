namespace BugTracker.Api.DTOs.Dashboard
{
    public class ProjectBugStatsResponse
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int BugCount { get; set; }
    }
}
