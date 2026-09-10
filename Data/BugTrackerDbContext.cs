using BugTracker.Api.Models;
using Microsoft.EntityFrameworkCore;


namespace BugTracker.Api.Data
{
    public class BugTrackerDbContext : DbContext
    {
        public BugTrackerDbContext(
            DbContextOptions<BugTrackerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects => Set<Project>();

        public DbSet<Bug> Bugs => Set<Bug>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>()
                .Property(p => p.Name)
                .HasMaxLength(150)
                .IsRequired();
            modelBuilder.Entity<Project>()
                .Property(p => p.Description)
                .HasMaxLength(1000);
            modelBuilder.Entity<Bug>()
                .Property(b => b.Title)
                .HasMaxLength(200)
                .IsRequired();
            modelBuilder.Entity<Bug>()
                .Property(b => b.Title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Bug>()
                .Property(b => b.Description)
                .HasMaxLength(4000)
                .IsRequired();

            modelBuilder.Entity<Bug>()
                .HasOne(b => b.Project)
                .WithMany(p => p.Bugs)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bug>()
                .HasQueryFilter(b => !b.IsDeleted);
        }
    }
}
    
