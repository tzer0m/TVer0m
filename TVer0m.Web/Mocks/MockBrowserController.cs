using TVer0m.Web.Services;

namespace TVer0m.Web.Mocks;

/// <summary>
/// Logs the home navigation instead of doing it, for testing off Linux.
/// </summary>
/// <param name="logger">The logger.</param>
public class MockBrowserController(ILogger<MockBrowserController> logger) : IBrowserController
{
    /// <summary>
    /// Logs that the browser would be sent back to the host page.
    /// </summary>
    /// <returns>The outcome of the command.</returns>
    public Task<CommandResult> HomeAsync()
    {
        logger.LogInformation("Mock: navigate browser to http://localhost:8090/host");
        return Task.FromResult(new CommandResult(true, "Mock navigated home"));
    }
}