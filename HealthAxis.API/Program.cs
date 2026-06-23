using HealthAxis.API.Models;
using HealthAxis.API.Data;
using HealthAxis.API.Middlewares;
using HealthAxis.API.Repositories.Implementations;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementation;
using HealthAxis.API.Services.Implementations;
using HealthAxis.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//----------------------------------------------------------
// Controllers
//----------------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "HealthAxis API",
            Version = "v1",
            Description = "Healthcare Appointment Management API"
        });

    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "bearer",
                document)] = []
        });
});

//----------------------------------------------------------
// Global Exception Handler
//----------------------------------------------------------

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

//----------------------------------------------------------
// Database
//----------------------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//----------------------------------------------------------
// Identity
//----------------------------------------------------------

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//----------------------------------------------------------
// JWT Authentication
//----------------------------------------------------------
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwt["Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)),

            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                Console.WriteLine($"Authorization Header: {context.Request.Headers.Authorization}");
                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ TOKEN VALIDATED");
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("❌ AUTH FAILED");
                Console.WriteLine(context.Exception.ToString());
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                Console.WriteLine($"Challenge Error: {context.Error}");
                Console.WriteLine($"Description: {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
//----------------------------------------------------------
// Dependency Injection
//----------------------------------------------------------

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IDoctorService, DoctorService>();

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();
builder.Services.AddScoped<IHealthRecordService, HealthRecordService>();

builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<IAuthService, AuthService>();

//----------------------------------------------------------
// AutoMapper
//----------------------------------------------------------

builder.Services.AddAutoMapper(
    cfg => { },
    AppDomain.CurrentDomain.GetAssemblies());

//----------------------------------------------------------
// CORS
//----------------------------------------------------------
// Allow the Admin Blazor app (and local API origins) to call this API during development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdmin",
        policy => policy
            .WithOrigins("https://localhost:7197", "https://localhost:7207")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

//----------------------------------------------------------
// Middleware
//----------------------------------------------------------

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    await RoleSeeder.SeedRolesAsync(roleManager);

    await AdminSeeder.SeedAdminAsync(userManager, roleManager);
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Apply CORS before authentication/authorization
app.UseCors("AllowAdmin");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();