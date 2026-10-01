using BugTracker.Api.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace BugTracker.Api.IntegrationTests;
public class AuthTests
{
    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsToken()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        var response = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "tester",
            password = "TesterPassword123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);

        Assert.Equal("tester", result.Username);
        Assert.Equal("Tester", result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }
    [Fact]
    public async Task Login_WithWronPassword_ReturnsUnAuthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "tester",
            password = "123"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task GetBugs_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/api/Bugs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

}