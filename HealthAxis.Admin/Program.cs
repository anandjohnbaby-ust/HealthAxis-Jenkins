using HealthAxis.Admin;
using HealthAxis.Admin.Authentication;
using HealthAxis.Admin.Configuration;
using HealthAxis.Admin.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// WebAssemblyHostBuilder automatically loads wwwroot/appsettings.json and
// wwwroot/appsettings.{Environment}.json into builder.Configuration.
// Binding it here removes the hardcoded absolute API/Angular URLs that
// previously lived directly in this file and in several components.
var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);
builder.Services.AddSingleton(appSettings);

// Register the message handler
builder.Services.AddScoped<AuthMessageHandler>();

// Configure HttpClient to automatically attach the JWT
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(appSettings.ApiBaseUrl)
    };
});

// Services
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<AdminService>();

// Authentication
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<CustomAuthenticationStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>());

await builder.Build().RunAsync();