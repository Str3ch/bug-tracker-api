using BugTracker.Api.DTOs.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BugTracker.Api.IntegrationTests
{
    public static class TestAuthHelper
    {
        public static async Task<string> LoginAsync(HttpClient client, string username, string password)
        {
            var response = await client.PostAsJsonAsync("/api/Auth/login", new
            {
                username,
                password
            });
            response.EnsureSuccessStatusCode();

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (auth == null)
            {
                throw new InvalidOperationException("Authentication response was empty");
            }
            return auth.Token;
        }
        public static void UseToken(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
