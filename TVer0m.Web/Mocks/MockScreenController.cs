using TVer0m.Web.Services;

namespace TVer0m.Web.Mocks;

/// <summary>
/// Logs the xset command instead of running it, for testing off Linux.
/// </summary>
/// <param name="logger">The logger.</param>
public class MockScreenController(ILogger<MockScreenController> logger) : IScreenController
{
    /// <summary>
    /// Logs the xset command that would toggle the display.
    /// </summary>
    /// <returns>The outcome of the command.</returns>
    public Task<CommandResult> ToggleAsync()
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Mock: xset dpms force <toggle>");
        }
        return Task.FromResult(new CommandResult(true, "Mock toggled screen"));
    }
}