using BugTracker.Api.Enums;

namespace BugTracker.Api.Models
{
    public class BugStatusHistory
    {
        public int Id { get; set; }
        public int BugId { get; set; }
        public Bug Bug { get; set; } = null!;
        public BugStatus OldStatus { get; set; }
        public BugStatus NewStatus { get; set; }
        public int ChangedById { get; set; }
        public User ChangedBy { get; set; } = null!;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
