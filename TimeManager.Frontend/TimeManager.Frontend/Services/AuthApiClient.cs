using Microsoft.AspNetCore.Mvc;

namespace TimeManager.Frontend.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _http;

        public AuthApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login?useCookies=true", new
            {
                email,
                password
            });

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return (false, problem?.Detail ?? "Login failed");
        }

        public async Task LogoutAsync()
       => await _http.PostAsync("/api/auth/logout", null);

        public async Task<(bool Success, string? Error)> RegisterAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/register", new
            {
                email,
                password
            });

            if (response.IsSuccessStatusCode)
                return (true, null);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return (false, problem?.Detail ?? "Registration failed.");
        }

        public async Task<UserInfo?> GetUserInfoAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<UserInfo>("/api/auth/manage/info");
            }
            catch
            {
                return null;
            }
        }

        public record UserInfo(string Email, bool IsEmailConfirmed);
    }
}
