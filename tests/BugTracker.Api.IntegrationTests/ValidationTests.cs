using BugTracker.Api.DTOs.Errors;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace BugTracker.Api.IntegrationTests
{
    public class ValidationTests
    {
        [Fact]
        public async Task CreateBug_WithInvalidData_ReturnsBadRequest()
        {
            using var factory = new CustomWebApplicationFactory();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            var token = await TestAuthHelper.LoginAsync(client, "tester", "TesterPassword123");

            TestAuthHelper.UseToken(client, token);

            var response = await client.PostAsJsonAsync("/api/Bugs", new
            {
                title = "",
                description = "x",
                priority = 1,
                severity = 1,
                projectId = 0
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var error = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();

            Assert.NotNull(error);

            Assert.Equal(400, error.StatusCode);

            Assert.Equal("Validation failed.", error.Message);
        }
    }
}
