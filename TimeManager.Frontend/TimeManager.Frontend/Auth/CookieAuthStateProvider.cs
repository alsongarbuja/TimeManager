using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TimeManager.Frontend.Services;

namespace TimeManager.Frontend.Auth
{
    public class CookieAuthStateProvider: AuthenticationStateProvider
    {
        private readonly AuthApiClient _authApi;
        private bool _authenticated = false;
        private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public CookieAuthStateProvider(AuthApiClient authApi)
        {
            _authApi = authApi;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userInfo = await _authApi.GetUserInfoAsync();

            if (userInfo == null)
            {
                return new AuthenticationState(_anonymous);
            }

            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, userInfo.Email),
                new (ClaimTypes.Email, userInfo.Email),
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            _authenticated = true;

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
        {
            var (success, error) = await _authApi.LoginAsync(email, password);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return (success, error);
        }

        public async Task LogoutAsync()
        {
            await _authApi.LogoutAsync();
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}
