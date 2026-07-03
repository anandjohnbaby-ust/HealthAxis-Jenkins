using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HealthAxis.Admin.Authentication
{
    public class AuthMessageHandler : DelegatingHandler
    {
        private const string TokenStorageKey = "token";

        private readonly IJSRuntime _js;

        public AuthMessageHandler(IJSRuntime js)
        {
            _js = js;
            // InnerHandler is set by the caller (Program.cs) for WASM.
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenStorageKey);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    token = token.Trim('"');
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (JSException)
            {
                // JS interop unavailable (e.g. prerendering) - proceed unauthenticated.
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}