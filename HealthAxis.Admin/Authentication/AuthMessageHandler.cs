using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace HealthAxis.Admin.Authentication
{
    public class AuthMessageHandler : DelegatingHandler
    {
        private readonly IJSRuntime _js;

        public AuthMessageHandler(IJSRuntime js)
        {
            _js = js;
            // InnerHandler will be set by the caller (Program.cs) for WASM
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _js.InvokeAsync<string>("localStorage.getItem", "token");

                if (!string.IsNullOrWhiteSpace(token))
                {
                    token = token.Trim('"');
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // ignore errors reading token
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
