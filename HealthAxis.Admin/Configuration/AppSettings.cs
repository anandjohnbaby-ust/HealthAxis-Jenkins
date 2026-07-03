namespace HealthAxis.Admin.Configuration
{
    /// <summary>
    /// Strongly typed application configuration, bound from wwwroot/appsettings.json
    /// (and its environment-specific overrides). Introduced to remove hardcoded
    /// absolute URLs/URIs from Program.cs, NavMenu.razor, Auth.razor and
    /// RedirectToAngular.razor, per the SonarQube "hardcoded absolute paths or URIs" findings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>Base address of the HealthAxis API, used by the shared HttpClient.</summary>
        public string ApiBaseUrl { get; set; } = string.Empty;

        /// <summary>Base address of the Angular front-end application.</summary>
        public string AngularAppBaseUrl { get; set; } = string.Empty;

        /// <summary>Full URL to the Angular login page.</summary>
        public string AngularLoginUrl => $"{AngularAppBaseUrl.TrimEnd('/')}/login";

        /// <summary>Full URL to the Angular logout page.</summary>
        public string AngularLogoutUrl => $"{AngularAppBaseUrl.TrimEnd('/')}/logout";
    }
}