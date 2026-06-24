using HealthAxis.Admin;
using HealthAxis.Admin.Authentication;
using HealthAxis.Admin.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<PatientService>();

// Configure HttpClient to point to your API. Used the API HTTPS URL from launchSettings.
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7207/")
});

// Enable Blazor Authorization
builder.Services.AddAuthorizationCore();

// Register Custom AuthenticationStateProvider
builder.Services.AddScoped<CustomAuthenticationStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>());

await builder.Build().RunAsync();