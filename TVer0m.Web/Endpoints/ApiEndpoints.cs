using TVer0m.Web.Services;

namespace TVer0m.Web.Endpoints;

/// <summary>
/// Maps the POST API used by the remote page.
/// </summary>
public static class ApiEndpoints
{
    /// <summary>
    /// Maps /api/key/{name}, /api/volume/{action} and /api/home.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same web application, for chaining.</returns>
    public static WebApplication MapApi(this WebApplication app)
    {
        app.MapPost("/api/key/{name}", async Task<IResult> (string name, IInputInjector injector) =>
        {
            if (!injector.IsValidKey(name))
            {
                return Results.BadRequest(new CommandResult(false, "Unknown key"));
            }
            return ToResult(await injector.SendKeyAsync(name));
        });
        app.MapPost("/api/volume/{action}", async Task<IResult> (string action, IVolumeController volume) =>
        {
            if (!volume.IsValidAction(action))
            {
                return Results.BadRequest(new CommandResult(false, "Unknown action"));
            }
            return ToResult(await volume.ApplyAsync(action));
        });
        app.MapPost("/api/home", async Task<IResult> (IBrowserController browser) => ToResult(await browser.HomeAsync()));
        app.MapPost("/api/power", async Task<IResult> (IScreenController screen) => ToResult(await screen.ToggleAsync()));
        return app;
    }

    /// <summary>
    /// Turns a command result into a JSON response, using 500 when the command failed.
    /// </summary>
    /// <param name="result">The command result.</param>
    /// <returns>The JSON response.</returns>
    private static IResult ToResult(CommandResult result)
    {
        return result.Success ? Results.Ok(result) : Results.Json(result, statusCode: StatusCodes.Status500InternalServerError);
    }
}