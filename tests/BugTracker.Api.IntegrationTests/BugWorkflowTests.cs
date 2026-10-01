using BugTracker.Api.DTOs.Bugs;
using BugTracker.Api.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace BugTracker.Api.IntegrationTests
{
    public class BugWorkflowTests
    {
        [Fact]
        public async Task Bug_CanGoThrough_Assignment_And_StatusWorkflow()
        {
            using var factory = new CustomWebApplicationFactory();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            var testerToken = await TestAuthHelper.LoginAsync(client, "tester", "TesterPassword123");

            TestAuthHelper.UseToken(client, testerToken);

            var createResponse = await client.PostAsJsonAsync("/api/Bugs", new
            {
                title = "Profile page does not load",
                description = "Profile page returns an error after opening",
                priority = (int)BugPriority.High,
                severity = (int)BugSeverity.Major,
                projectId = 1
            });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var bug = await createResponse.Content.ReadFromJsonAsync<BugResponse>();

            Assert.NotNull(bug);
            Assert.Equal("Open", bug.Status);
            Assert.Equal("tester", bug.CreatedByUsername);

            var adminToken = await TestAuthHelper.LoginAsync(client, "admin", "AdminPassword123");

            TestAuthHelper.UseToken(client, adminToken);

            var assignResponse = await client.PutAsJsonAsync($"/api/Bugs/{bug.Id}/assign", new
            {
                userId = 2
            });

            Assert.Equal(HttpStatusCode.NoContent, assignResponse.StatusCode);

            var developerToken = await TestAuthHelper.LoginAsync(client, "developer", "DeveloperPassword123");

            TestAuthHelper.UseToken(client, developerToken);

            var statusResponse = await client.PutAsJsonAsync($"/api/Bugs/{bug.Id}/status", new
            {
                status = (int)BugStatus.InProgress
            });
            
            Assert.Equal(HttpStatusCode.NoContent, statusResponse.StatusCode);

            var updateBug = await client.GetFromJsonAsync<BugResponse>($"/api/Bugs/{bug.Id}");

            Assert.NotNull(updateBug);
            Assert.Equal("InProgress",updateBug.Status);
            Assert.Equal(2, updateBug.AssignedToId);
            Assert.Equal("developer", updateBug.AssignedToUsername);

            var history = await client.GetFromJsonAsync<List<BugStatusHistoryResponse>>(
                $"/api/Bugs/{bug.Id}/status-history");

            Assert.NotNull(history);
            Assert.Single(history);
            Assert.Equal("Open", history[0].OldStatus);
            Assert.Equal("InProgress", history[0].NewStatus);
            Assert.Equal("developer", history[0].ChangedByUsername);
        }
        [Fact]
        public async Task AssignBug_ToTester_ReturnBadRequest()
        {
            using var factory = new CustomWebApplicationFactory();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            var testerToken = await TestAuthHelper.LoginAsync(client, "tester", "TesterPassword123");

            TestAuthHelper.UseToken(client, testerToken);
            var createResponse = await client.PostAsJsonAsync("/api/Bugs", new
            {
                title = "Assignment test bug",
                description = "Bug used to test invalid assignment",
                priority = (int)BugPriority.Medium,
                severity = (int)BugSeverity.Major,
                projectId = 1
            });
            var bug = await createResponse.Content.ReadFromJsonAsync<BugResponse>();

            Assert.NotNull(bug);

            var adminToken = await TestAuthHelper.LoginAsync(client, "admin", "AdminPassword123");

            TestAuthHelper.UseToken(client, adminToken);

            var response = await client.PutAsJsonAsync($"/api/Bugs/{bug.Id}/assign", new
            {
                userId = 3
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        }
}
