using TVer0m.Web.Services;

namespace TVer0m.Web.Mocks;

/// <summary>
/// Logs the pactl command instead of running it, for testing off Linux.
/// </summary>
/// <param name="logger">The logger.</param>
public class MockVolumeController(ILogger<MockVolumeController> logger) : IVolumeController
{
    /// <summary>
    /// Checks whether a volume action is on the whitelist.
    /// </summary>
    /// <param name="action">The action from the request.</param>
    /// <returns>True if the action is one of up, down or mute.</returns>
    public bool IsValidAction(string action)
    {
        return VolumeController.BuildArguments(action, 0) is not null;
    }

    /// <summary>
    /// Logs the pactl command that would be run for a whitelisted action.
    /// </summary>
    /// <param name="action">The action from the request.</param>
    /// <returns>The outcome of the command.</returns>
    public Task<CommandResult> ApplyAsync(string action)
    {
        IReadOnlyList<string>? arguments = VolumeController.BuildArguments(action, 0);
        if (arguments is null)
        {
            return Task.FromResult(new CommandResult(false, "Unknown action"));
        }
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Mock: pactl {Arguments}", string.Join(' ', arguments));
        }
        return Task.FromResult(new CommandResult(true, "Mock applied " + action));
    }
}