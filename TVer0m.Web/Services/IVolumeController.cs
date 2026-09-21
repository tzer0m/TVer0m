namespace TVer0m.Web.Services;

/// <summary>
/// Changes the default audio output volume.
/// </summary>
public interface IVolumeController
{
    /// <summary>
    /// Checks whether a volume action is on the whitelist.
    /// </summary>
    /// <param name="action">The action from the request.</param>
    /// <returns>True if the action is one of up, down or mute.</returns>
    bool IsValidAction(string action);

    /// <summary>
    /// Applies a whitelisted volume action to the default sink.
    /// </summary>
    /// <param name="action">The action from the request.</param>
    /// <returns>The outcome of the command.</returns>
    Task<CommandResult> ApplyAsync(string action);
}