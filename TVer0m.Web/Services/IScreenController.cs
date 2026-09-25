namespace TVer0m.Web.Services;

/// <summary>
/// Turns the display on or off.
/// </summary>
public interface IScreenController
{
    /// <summary>
    /// Turns the display off if it is on, or on if it is off.
    /// </summary>
    /// <returns>The outcome of the command.</returns>
    Task<CommandResult> ToggleAsync();
}