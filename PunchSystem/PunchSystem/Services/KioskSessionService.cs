using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.JSInterop;

namespace PunchSystem.Services
{
    public class KioskSessionService(HttpClient http, IJSRuntime jsRuntime)
    {
        private const string TokenKey = "tc_kiosk_token";

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                var token = await jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);
                return string.IsNullOrEmpty(token) ? null : token;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> HasSavedTokenAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        /// Loads the saved token (if any) and attaches it to the shared HttpClient
        /// as a Bearer Authorization header. Call this once when a page loads,
        /// before making any authenticated API calls (e.g. /api/punch).
        /// </summary>
        public async Task<bool> RestoreSessionAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        /// <summary>
        /// Wipes the saved token, e.g. after the backend returns 401 on a punch
        /// (kiosk was deprovisioned or its token expired/was revoked).
        /// </summary>
        public async Task ClearTokenAsync()
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            }
            catch
            {
                
            }

            http.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<string> InitKioskAsync(string uniqueId)
        {
            try
            {
                var res = await http.PostAsJsonAsync("api/kiosk/init", new KioskInitRequest
                {
                    UniqueId = uniqueId
                });

                if (res.IsSuccessStatusCode) return "";

                var error = await res.Content.ReadFromJsonAsync<ErrorResponse>();
                return error?.message ?? "Given Id is not authorized to setup kiosk";
            }
            catch
            {
                return "Unable to reach the server. Please check your connection and try again.";
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetDeptOptionsAsync()
        {
            try
            {
                var res = await http.GetAsync("api/kiosk/departments");
                if (!res.IsSuccessStatusCode) return [];

                var data = await res.Content.ReadFromJsonAsync<KioskDeptResponse>();
                return data?.departments ?? [];
            }
            catch
            {
                return [];
            }
        }

        public async Task<(bool Success, string Error)> ProvisionKioskAsync(string name, string description, int departmentId)
        {
            try
            {
                var res = await http.PostAsJsonAsync("api/kiosk/provision", new KioskProvisionRequest
                {
                    Name = name,
                    Description = description,
                    DepartmentId = departmentId
                });

                if (!res.IsSuccessStatusCode)
                {
                    var error = await res.Content.ReadFromJsonAsync<ErrorResponse>();
                    return (false, error?.message ?? "Unable to provision this kiosk. Please try again.");
                }

                var session = await res.Content.ReadFromJsonAsync<KioskSessionResponse>();
                if (session is null || string.IsNullOrEmpty(session.Token))
                {
                    return (false, "Provisioning succeeded but no session token was returned. Contact IT.");
                }

                await jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, session.Token);
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", session.Token);

                return (true, "");
            }
            catch
            {
                return (false, "Unable to reach the server. Please check your connection and try again.");
            }
        }
    }

    public class KioskInitRequest
    {
        public string UniqueId { get; set; } = string.Empty;
    }

    public class KioskProvisionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
    }

    public record KioskSessionResponse(string Token);
    public record ErrorResponse(string message);
    public record KioskDeptResponse(IEnumerable<SelectListItem> departments);
}