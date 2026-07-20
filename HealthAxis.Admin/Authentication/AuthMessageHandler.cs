using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace HealthAxis.Admin.Authentication
{
    public class AuthMessageHandler : DelegatingHandler
    {
        private readonly IJSRuntime _js;

        public AuthMessageHandler(IJSRuntime js)
        {
            _js = js;
            // InnerHandler will be set by Program.cs
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Read the same key used by Angular
                var token = await _js.InvokeAsync<string>(
                    "localStorage.getItem",
                    "accessToken");

                if (!string.IsNullOrWhiteSpace(token))
                {
                    token = token.Trim('"');

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // Ignore errors reading token
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}