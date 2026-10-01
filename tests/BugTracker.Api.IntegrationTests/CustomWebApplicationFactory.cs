using BugTracker.Api.Data;
using BugTracker.Api.Enums;
using BugTracker.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BugTracker.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        public CustomWebApplicationFactory()
        {
            Environment.SetEnvironmentVariable(
               "Jwt__Key",
               "Integration-Test-Super-Secret-Key-12345678901234567890");

            Environment.SetEnvironmentVariable(
                "Jwt__Issuer",
                "BugTracker.Api");

            Environment.SetEnvironmentVariable(
                "Jwt__Audience",
                "BugTracker.Client");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

           
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<BugTrackerDbContext>>();
                services.RemoveAll<BugTrackerDbContext>();
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<BugTrackerDbContext>(options => options.UseSqlite(_connection));
            });
        }
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BugTrackerDbContext>();

            context.Database.EnsureCreated();
            SeedDatabase(context);

            return host;
        }
        private static void SeedDatabase(BugTrackerDbContext context)
        {
            var admin = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123"),
                Role = UserRole.Admin
            };
            var developer = new User
            {
                Id = 2,
                Username = "developer",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("DeveloperPassword123"),
                Role = UserRole.Developer
            };
            var tester = new User
            {
                Id = 3,
                Username = "tester",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TesterPassword123"),
                Role = UserRole.Tester
            };
            var project = new Project
            {
                Id = 1,
                Name = "Integration Test Project",
                Description = "Project used by integration tests"
            };
            context.Users.AddRange(admin,developer,tester);

            context.Projects.Add(project);
            context.SaveChanges();
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection?.Dispose();  
            }
        }
    }
}
