namespace TVer0m.Web.Authentication;

/// <summary>
/// Settings for signing in with Pocket ID.
/// </summary>
public class OidcOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Oidc";

    /// <summary>
    /// Gets or sets the Pocket ID address.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how many days a sign in lasts.
    /// </summary>
    public int SessionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the address of the proxy allowed to set forwarded headers.
    /// </summary>
    public string ProxyAddress { get; set; } = string.Empty;
}