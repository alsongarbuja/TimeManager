using Microsoft.JSInterop;

namespace TimeManager.Frontend.Services
{
    public class TokenStore
    {
        private readonly IJSRuntime _js;

        public TokenStore(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SetTokensAsync(string accessToken, string refreshToken)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "access_token", accessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", "refresh_token", refreshToken);
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", "access_token");
        }

        public async Task<string?> GetRefreshTokenAsync()
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", "refresh_token");
        }

        public async Task ClearAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "access_token");
            await _js.InvokeVoidAsync("localStorage.removeItem", "refresh_token");
        }
    }
}