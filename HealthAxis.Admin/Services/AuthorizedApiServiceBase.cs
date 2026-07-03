using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace HealthAxis.Admin.Services
{
    /// <summary>
    /// Shared base for API services that need a bearer token attached to outgoing requests.
    /// Extracted from AdminService/DoctorService/PatientService, which each duplicated
    /// this exact logic.
    /// </summary>
    public abstract class AuthorizedApiServiceBase
    {
        private const string TokenStorageKey = "token";

        protected HttpClient Http { get; }

        private readonly IJSRuntime _js;

        protected AuthorizedApiServiceBase(HttpClient http, IJSRuntime js)
        {
            Http = http;
            _js = js;
        }

        protected async Task AttachAuthHeaderAsync()
        {
            try
            {
                var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenStorageKey);

                Http.DefaultRequestHeaders.Authorization = !string.IsNullOrWhiteSpace(token)
                    ? new AuthenticationHeaderValue("Bearer", token.Trim('"'))
                    : null;
            }
            catch (JSException)
            {
                Http.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}