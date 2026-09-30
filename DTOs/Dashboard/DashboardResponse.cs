namespace BugTracker.Api.DTOs.Dashboard
{
    public class DashboardResponse
    {
        public int TotalBugs { get; set; }
        public int UnassignedBugs { get; set; }
        public int TotalProjects { get; set; }
        public int TotalUsers { get; set; }
        public int OpenBugs { get; set; }
        public int InProgressBugs { get; set; }
        public int ResolvedBugs { get; set; }
        public int ClosedBugs { get; set; }
        public int ReopenedBugs { get; set; }
        public int CriticalPriorityBugs { get; set; }
        public int BlockerSeverityBugs { get; set; }
        public List<ProjectBugStatsResponse> BugsByProject { get; set; } =
            new();
        public List<DeveloperWorkloadResponse> DeveloperWorkload { get; set; } = 
            new();
    }
}
