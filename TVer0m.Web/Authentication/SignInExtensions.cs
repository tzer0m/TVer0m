using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;

namespace TVer0m.Web.Authentication;

/// <summary>
/// Registers and applies the Pocket ID sign in.
/// </summary>
public static class SignInExtensions
{
    /// <summary>
    /// Adds forwarded headers, cookie and OpenID Connect sign in, and the rule that non-local requests must be signed in.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The same builder.</returns>
    public static WebApplicationBuilder AddSignIn(this WebApplicationBuilder builder)
    {
        OidcOptions oidc = builder.Configuration.GetSection(OidcOptions.SectionName).Get<OidcOptions>() ?? new OidcOptions();
        builder.Services.Configure<ForwardedHeadersOptions>(options => ConfigureForwardedHeaders(options, oidc));
        builder.Services.AddAuthentication(options => ConfigureSchemes(options)).AddCookie(options => ConfigureCookie(options, oidc)).AddOpenIdConnect(options => ConfigureOpenIdConnect(options, oidc));
        builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAssertion(CanAccess).Build());
        return builder;
    }

    /// <summary>
    /// Adds the forwarded headers and authentication middleware.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same application.</returns>
    public static WebApplication UseSignIn(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    /// <summary>
    /// Trusts forwarded headers from the configured proxy only.
    /// </summary>
    /// <param name="options">The forwarded headers options.</param>
    /// <param name="oidc">The sign in settings.</param>
    private static void ConfigureForwardedHeaders(ForwardedHeadersOptions options, OidcOptions oidc)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Add(IPAddress.Parse(oidc.ProxyAddress));
    }

    /// <summary>
    /// Uses the cookie to hold the session and Pocket ID to sign in.
    /// </summary>
    /// <param name="options">The authentication options.</param>
    private static void ConfigureSchemes(AuthenticationOptions options)
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    }

    /// <summary>
    /// Makes the sign in cookie last for the configured number of days.
    /// </summary>
    /// <param name="options">The cookie options.</param>
    /// <param name="oidc">The sign in settings.</param>
    private static void ConfigureCookie(CookieAuthenticationOptions options, OidcOptions oidc)
    {
        options.Cookie.Name = "TVer0m.Auth";
        options.ExpireTimeSpan = TimeSpan.FromDays(oidc.SessionDays);
        options.SlidingExpiration = true;
    }

    /// <summary>
    /// Points the OpenID Connect handler at Pocket ID.
    /// </summary>
    /// <param name="options">The OpenID Connect options.</param>
    /// <param name="oidc">The sign in settings.</param>
    private static void ConfigureOpenIdConnect(OpenIdConnectOptions options, OidcOptions oidc)
    {
        options.Authority = oidc.Authority;
        options.ClientId = oidc.ClientId;
        options.ClientSecret = oidc.ClientSecret;
        options.ResponseType = "code";
        options.UsePkce = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Events.OnRedirectToIdentityProvider = RejectApiRedirect;
        options.Events.OnTicketReceived = KeepSignedIn;
    }

    /// <summary>
    /// Allows local requests and signed in users.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <returns>True when access is allowed.</returns>
    private static bool CanAccess(AuthorizationHandlerContext context)
    {
        bool isLocal = context.Resource is HttpContext http && LocalRequest.IsLocal(http);
        return isLocal || context.User.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Returns 401 to API calls instead of redirecting them to Pocket ID.
    /// </summary>
    /// <param name="context">The redirect context.</param>
    /// <returns>A completed task.</returns>
    private static Task RejectApiRedirect(RedirectContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.HandleResponse();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Makes the session survive closing the browser.
    /// </summary>
    /// <param name="context">The ticket context.</param>
    /// <returns>A completed task.</returns>
    private static Task KeepSignedIn(TicketReceivedContext context)
    {
        context.Properties!.IsPersistent = true;
        return Task.CompletedTask;
    }
}