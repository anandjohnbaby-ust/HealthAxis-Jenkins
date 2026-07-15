using HealthAxis.Admin;
using HealthAxis.Admin.Authentication;
using HealthAxis.Admin.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register the message handler
builder.Services.AddScoped<AuthMessageHandler>();

// Configure HttpClient to automatically attach the JWT
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var handler = sp.GetRequiredService<AuthMessageHandler>();

    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(
            configuration["ApiSettings:BaseUrl"]
            ?? throw new InvalidOperationException(
                "ApiSettings:BaseUrl is not configured."))
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