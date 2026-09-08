namespace BugTracker.Api.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ?Description { get; set; }
        public ICollection<Bug> Bugs { get; set; }
            = new List<Bug>();
    }
}
