namespace TVer0m.Web.Endpoints;

/// <summary>
/// Maps the health check.
/// </summary>
public static class HealthEndpoints
{
    /// <summary>
    /// Maps /health, which needs no sign in.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same web application, for chaining.</returns>
    public static WebApplication MapHealth(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();
        return app;
    }
}