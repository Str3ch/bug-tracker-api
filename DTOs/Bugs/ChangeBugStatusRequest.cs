using BugTracker.Api.Enums;

namespace BugTracker.Api.DTOs.Bugs
{
    public class ChangeBugStatusRequest
    {
        public BugStatus Status { get; set; } 
    }
}
