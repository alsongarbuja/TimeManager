using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TimeManager.Frontend.State
{
    public class BackendAuthenticationStateProvider: AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public BackendAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<UserStatus>("/auth/status");

                if (response == null || !response.IsAuthenticated)
                {
                    return new AuthenticationState(_anonymous);
                }

                var identity = new ClaimsIdentity(
                    response.Claims.Select(c => new Claim(c.Key, c.Value)), "CookieAuth");

                return new AuthenticationState(new ClaimsPrincipal(identity));
            } 
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyUserAuthentication() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        private class UserStatus
        {
            public bool IsAuthenticated { get; set; }
            public string? Username { get; set; }
            public Dictionary<string, string> Claims { get; set; } = new();
        }
    }
}
