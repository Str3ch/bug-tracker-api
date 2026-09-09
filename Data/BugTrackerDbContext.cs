using BugTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BugTracker.Api.Data
{
    public class BugTrackerDbContext:DbContext
    {
        public BugTrackerDbContext(
            DbContextOptions<BugTrackerDbContext> options)
            :base(options) 
        {
        }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Bug> Bugs => Set<Bug>();

    }
}
