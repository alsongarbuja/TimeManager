namespace PunchSystem.Services
{
    public class KioskSessionService
    {
        private readonly HttpClient _http;
        private string? _token;
        private DateTime? _expiresAt = DateTime.MinValue;
    
        public KioskSessionService(HttpClient http)
        {
            _http = http;
        }

        public bool IsInitialized => _token != null && DateTime.UtcNow < _expiresAt?.AddMinutes(-5);

        public async Task<bool> InitializedAsync()
        {
            try
            {
                var res = await _http.PostAsync("api/kiosk/init", null);
                if (!res.IsSuccessStatusCode) return false;

                var session = await res.Content.ReadFromJsonAsync<KioskSessionResponse>();
                if (session is null) return false;

                _token = session.Token;
                _expiresAt = session.ExpiresAt;

                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                return true;
            } catch
            {
                return false;
            }
        }

        public void Invalidate() => _token = null;
    }

    public record KioskSessionResponse(
        string Token,
        string KioskName,
        int DepartmentId,
        string DepartmentName,
        DateTime ExpiresAt
    );
}
