namespace TodoListApp.WebApp.Services;

/// <summary>
/// The settings of the connection to the web API, read from the "WebApi" section of the configuration.
/// </summary>
public class WebApiOptions
{
    /// <summary>The name of the configuration section.</summary>
    public const string SectionName = "WebApi";

    /// <summary>Gets or sets the base address of the web API; it must end with a slash.</summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>Gets or sets the issuer of the access tokens sent to the web API.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Gets or sets the audience of the access tokens sent to the web API.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Gets or sets the key the access tokens are signed with; the web API validates tokens with the same key.</summary>
    public string SecurityKey { get; set; } = string.Empty;

    /// <summary>Gets or sets how long an access token is valid, in minutes.</summary>
    public int TokenLifetimeMinutes { get; set; } = 5;
}
