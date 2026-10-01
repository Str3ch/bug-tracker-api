using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BugTracker.Api.IntegrationTests
{
    public class AuthorizationTests
    {
        [Fact]
        public async Task Tester_CannotAccessDashboard()
        {
            using var factory = new CustomWebApplicationFactory();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            var token = await TestAuthHelper.LoginAsync(client, "tester", "TesterPassword123");

            TestAuthHelper.UseToken(client, token);

            var response = await client.GetAsync("/api/Dashboard");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        [Fact]
        public async Task Admin_CanAccessDashboard()
        {
            using var factory = new CustomWebApplicationFactory();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            var token = await TestAuthHelper.LoginAsync(client, "admin", "AdminPassword123");

            TestAuthHelper.UseToken(client, token);

            var response = await client.GetAsync("/api/Dashboard");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        }
}
