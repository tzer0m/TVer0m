using TVer0m.Web.Services;

namespace TVer0m.Web.Mocks;

/// <summary>
/// Logs the xdotool command instead of running it, for testing off Linux.
/// </summary>
/// <param name="logger">The logger.</param>
public class MockInputInjector(ILogger<MockInputInjector> logger) : IInputInjector
{
    /// <summary>
    /// Checks whether a key name is on the whitelist.
    /// </summary>
    /// <param name="name">The key name from the request.</param>
    /// <returns>True if the name is one of up, down, left, right or ok.</returns>
    public bool IsValidKey(string name)
    {
        return InputInjector.BuildArguments(name) is not null;
    }

    /// <summary>
    /// Logs the xdotool command that would be run for a whitelisted key.
    /// </summary>
    /// <param name="name">The key name from the request.</param>
    /// <returns>The outcome of the command.</returns>
    public Task<CommandResult> SendKeyAsync(string name)
    {
        IReadOnlyList<string>? arguments = InputInjector.BuildArguments(name);
        if (arguments is null)
        {
            return Task.FromResult(new CommandResult(false, "Unknown key"));
        }
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Mock: xdotool {Arguments}", string.Join(' ', arguments));
        }
        return Task.FromResult(new CommandResult(true, "Mock sent " + name));
    }
}