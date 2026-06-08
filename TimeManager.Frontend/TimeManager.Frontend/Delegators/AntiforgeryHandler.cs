using Microsoft.JSInterop;

namespace TimeManager.Frontend.Delegators
{
    public class AntiforgeryHandler: DelegatingHandler
    {
        private readonly IJSRuntime _jsRunTime;

        public AntiforgeryHandler(IJSRuntime jSRuntime)
        {
            _jsRunTime = jSRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
            {
                var token = await _jsRunTime.InvokeAsync<string>("eval", "document.querySelector('meta[name=\"x-antiforgery-token\"]).getAttribute('content')");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("X-XSRF-TOKEN", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
