namespace BugTracker.Api.Enums
{
    public enum AuditAction
    {
        BugCreated,
        BugUpdated, 
        BugDeleted, 
        BugAasigned,
        BugUnassigned,
        BugStatusChanged,
        CommentAdded,
        TagAdded,
        TagRemoved,
        TagCreated,
        TagDeleted,
        UserRoleChanged,
        ProjectCreated,
        ProjectUpdated,
        ProjectDeleted
    }
}
