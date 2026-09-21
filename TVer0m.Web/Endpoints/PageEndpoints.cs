using TVer0m.Web.Detection;

namespace TVer0m.Web.Endpoints;

/// <summary>
/// Maps the entry route and the host and remote pages.
/// </summary>
public static class PageEndpoints
{
    /// <summary>
    /// Maps /, /host and /remote.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same web application, for chaining.</returns>
    public static WebApplication MapPages(this WebApplication app)
    {
        app.MapGet("/", IResult (HttpContext context, DeviceDetector detector, IWebHostEnvironment environment) => Route(context, detector, environment));
        app.MapGet("/host", IResult (IWebHostEnvironment environment) => PageFile(environment, "host.html"));
        app.MapGet("/remote", IResult (IWebHostEnvironment environment) => PageFile(environment, "remote.html"));
        return app;
    }

    /// <summary>
    /// Sends the visitor to the host or remote page, or serves the interstitial when unsure.
    /// </summary>
    /// <param name="context">The current request.</param>
    /// <param name="detector">The device detector.</param>
    /// <param name="environment">The web host environment.</param>
    /// <returns>A redirect or the interstitial page.</returns>
    private static IResult Route(HttpContext context, DeviceDetector detector, IWebHostEnvironment environment)
    {
        string? mode = context.Request.Query["mode"];
        DeviceTarget target = detector.Detect(context.Request.Headers.UserAgent.ToString(), mode);
        return target switch { DeviceTarget.Host => Results.Redirect("/host"), DeviceTarget.Remote => Results.Redirect("/remote"), _ => PageFile(environment, "interstitial.html") };
    }

    /// <summary>
    /// Serves an HTML file from wwwroot.
    /// </summary>
    /// <param name="environment">The web host environment.</param>
    /// <param name="fileName">The file name inside wwwroot.</param>
    /// <returns>The file as an HTML response.</returns>
    private static IResult PageFile(IWebHostEnvironment environment, string fileName)
    {
        return Results.File(Path.Combine(environment.WebRootPath, fileName), "text/html");
    }
}