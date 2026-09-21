namespace TVer0m.Web.Services;

/// <summary>
/// Sends key presses to the focused window.
/// </summary>
public interface IInputInjector
{
    /// <summary>
    /// Checks whether a key name is on the whitelist.
    /// </summary>
    /// <param name="name">The key name from the request.</param>
    /// <returns>True if the name is one of up, down, left, right or ok.</returns>
    bool IsValidKey(string name);

    /// <summary>
    /// Sends a whitelisted key to the focused window.
    /// </summary>
    /// <param name="name">The key name from the request.</param>
    /// <returns>The outcome of the command.</returns>
    Task<CommandResult> SendKeyAsync(string name);
}