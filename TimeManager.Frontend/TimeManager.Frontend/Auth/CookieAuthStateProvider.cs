using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TimeManager.Frontend.Services;

namespace TimeManager.Frontend.Auth
{
    public class CookieAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthApiClient _authApi;
        private ClaimsPrincipal? _cachedUser;

        public CookieAuthStateProvider(AuthApiClient authApi)
        {
            _authApi = authApi;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedUser != null)
                return new AuthenticationState(_cachedUser);

            _cachedUser = await _authApi.GetClaimsPrincipalAsync();
            return new AuthenticationState(_cachedUser);
        }

        public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
        {
            var (success, error) = await _authApi.LoginAsync(email, password);
            if (success)
            {
                _cachedUser = await _authApi.GetClaimsPrincipalAsync();
                NotifyAuthenticationStateChanged(
                    Task.FromResult(new AuthenticationState(_cachedUser)));
            }
            return (success, error);
        }

        public async Task LogoutAsync()
        {
            await _authApi.LogoutAsync();
            _cachedUser = null;
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity()))));
        }
    }
    //public class CookieAuthStateProvider: AuthenticationStateProvider
    //{
    //    private readonly AuthApiClient _authApi;
    //    private readonly IHttpContextAccessor _httpContextAccessor;
    //    private ClaimsPrincipal? _cachedUser;

    //    public CookieAuthStateProvider(AuthApiClient authApi, IHttpContextAccessor httpContextAccessor)
    //    {
    //        _authApi = authApi;
    //        _httpContextAccessor = httpContextAccessor;
    //    }

    //    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    //    {
    //        if (_cachedUser != null) {
    //            return new AuthenticationState(_cachedUser);
    //        }
    //        var user = _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    //        if (user.Identity?.IsAuthenticated == true)
    //        {
    //            _cachedUser = user;
    //        }

    //        return new AuthenticationState(user);
    //    }

    //    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    //    {
    //        var (success, error) = await _authApi.LoginAsync(email, password);
    //        if (success)
    //        {
    //            var identity = new ClaimsIdentity(new[] { 
    //                new Claim(ClaimTypes.Email, email)
    //            }, authenticationType: "Cookie");

    //            _cachedUser = new ClaimsPrincipal(identity);
    //            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    //        }
    //        return (success, error);
    //    }

    //    public async Task LogoutAsync()
    //    {
    //        await _authApi.LogoutAsync();
    //        _cachedUser = null;
    //        NotifyAuthenticationStateChanged(
    //            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    //    }
    //}
}
