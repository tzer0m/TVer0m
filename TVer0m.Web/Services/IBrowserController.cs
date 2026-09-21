namespace TVer0m.Web.Services;

/// <summary>
/// Controls the kiosk browser.
/// </summary>
public interface IBrowserController
{
    /// <summary>
    /// Navigates the browser back to the host page.
    /// </summary>
    /// <returns>The outcome of the command.</returns>
    Task<CommandResult> HomeAsync();
}