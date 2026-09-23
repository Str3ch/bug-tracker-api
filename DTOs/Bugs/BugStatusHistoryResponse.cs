namespace BugTracker.Api.DTOs.Bugs
{
    public class BugStatusHistoryResponse
    {
        public int Id { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedById { get; set; }
        public string ChangedByUsername { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } 
    }
}
