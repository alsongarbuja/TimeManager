using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TimeManager.Frontend.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void ForwardCookies()
        {
            var cookieHeader = _httpContextAccessor.HttpContext?.Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                _http.DefaultRequestHeaders.Remove("Cookie");
                _http.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }
        }

        public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login?useCookies=true", new
            {
                email,
                password
            });

            if (!response.IsSuccessStatusCode)
                return (false, await response.Content.ReadAsStringAsync());

            // This now works because it's called during a real HTTP request
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {   
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    foreach (var cookie in cookies)
                    {
                        httpContext.Response.Headers.Append("Set-Cookie", cookie);
                    }
                }
            }

            return (true, null);
        }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync()
        {
            ForwardCookies();
            try
            {
                var response = await _http.GetAsync("api/auth/me");
                if (!response.IsSuccessStatusCode)
                {
                    return new ClaimsPrincipal(new ClaimsIdentity());
                }
                var user = await response.Content.ReadFromJsonAsync<UserInfoDto>();
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user!.Email ?? ""),
                    new Claim(ClaimTypes.Name, user!.Name ?? "")
                };
                foreach (var role in user.Roles ?? [])
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                return new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookie"));
            }
            catch (Exception ex) {
                Console.WriteLine($"GetClaimsPrincipalAync failed: {ex.Message}");
                return new ClaimsPrincipal(new ClaimsIdentity());
            }
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

        public record UserInfoDto(string? Email, string? Name, IEnumerable<string>? Roles);
    }
}
