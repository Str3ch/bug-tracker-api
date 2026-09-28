namespace BugTracker.Api.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<BugTag> BagTags { get; set; }
        = new List<BugTag>();
    }
}
