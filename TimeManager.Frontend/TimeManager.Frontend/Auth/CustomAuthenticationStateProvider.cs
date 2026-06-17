using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TimeManager.Frontend.Services;

namespace TimeManager.Frontend.Auth
{
    public class CustomAuthenticationStateProvider: AuthenticationStateProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenStore _tokenStore;
        private ClaimsPrincipal _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(IHttpClientFactory httpClientFactory, TokenStore tokenStore)
        {
            _httpClientFactory = httpClientFactory;
            _tokenStore = tokenStore;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedUser.Identity?.IsAuthenticated == true)
                return new AuthenticationState(_cachedUser);

            string? token = null;

            try
            {
                token = await _tokenStore.GetAccessTokenAsync();
            }
            catch (InvalidOperationException)
            {
                // JS interop not available yet (prerender phase), return unauthenticated
                return Unauthenticated();
            }

            try
            {
                var http = _httpClientFactory.CreateClient("BackendApi");
                var userInfo = await http.GetFromJsonAsync<UserInfo>("/api/auth/manage/info");

                Console.WriteLine(userInfo);

                if (userInfo == null)
                    return Unauthenticated();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, userInfo.Email)
                };

                foreach(var role in userInfo.Roles) {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                _cachedUser = new ClaimsPrincipal(new ClaimsIdentity(claims));

                return new AuthenticationState(_cachedUser);
            } catch (Exception e)
            {
                Console.WriteLine(e);
                return Unauthenticated();
            }
        }

        public void NotifyUserAuthentication(string email)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, email) };
            var identity = new ClaimsIdentity(claims, "ServerIdentity");
            _cachedUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
        }

        public void NotifyUserLoggedOut()
        {
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
        }

        private AuthenticationState Unauthenticated() =>
        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public class UserInfo
    {
        public string Email { get; set;  }
        public string[] Roles { get; set; }
        public string Password { get; set; }

        public bool isAuthenticated { get; set; }
    }
}
