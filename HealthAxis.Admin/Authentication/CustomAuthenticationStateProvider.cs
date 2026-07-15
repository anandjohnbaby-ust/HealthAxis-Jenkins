using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace HealthAxis.Admin.Authentication
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;

        public CustomAuthenticationStateProvider(IJSRuntime js)
        {
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            Console.WriteLine("GetAuthenticationStateAsync CALLED");

            var token = await _js.InvokeAsync<string>("localStorage.getItem", "token");

            Console.WriteLine("TOKEN = " + token);

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("NO TOKEN");

                return new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(token);

            Console.WriteLine("CLAIMS = " + claims.Count);

            foreach (var c in claims)
            {
                Console.WriteLine($"{c.Type} = {c.Value}");
            }

            var identity = new ClaimsIdentity(claims, "jwt");

            Console.WriteLine("Authenticated = " + identity.IsAuthenticated);

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private static List<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();

            try
            {
                var payload = jwt.Split('.')[1];

                switch (payload.Length % 4)
                {
                    case 2:
                        payload += "==";
                        break;
                    case 3:
                        payload += "=";
                        break;
                }

                var jsonBytes = Convert.FromBase64String(payload);

                var keyValuePairs =
                    JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

                if (keyValuePairs is null)
                    return claims;

                foreach (var kvp in keyValuePairs)
                {
                    if (kvp.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var value in kvp.Value.EnumerateArray())
                        {
                            claims.Add(new Claim(kvp.Key, value.ToString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
                    }
                }
            }
            catch
            {
                // Invalid JWT
            }

            return claims;
        }

        public void NotifyUserLoggedIn(string token)
        {
            var claims = ParseClaimsFromJwt(token);

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "jwt");

            var currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(
                Task.FromResult(
                    new AuthenticationState(currentUser)));
        }

        public void NotifyUserLoggedOut()
        {
            var currentUser = new ClaimsPrincipal(
                new ClaimsIdentity());

            NotifyAuthenticationStateChanged(
                Task.FromResult(
                    new AuthenticationState(currentUser)));
        }
    }
}