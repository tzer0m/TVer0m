namespace TVer0m.Web.Services;

/// <summary>
/// Turns the display on or off using xset DPMS.
/// </summary>
/// <param name="runner">The process runner.</param>
public class ScreenController(IProcessRunner runner) : IScreenController
{
    /// <summary>
    /// Turns the display off if it is on, or on if it is off.
    /// </summary>
    /// <returns>The outcome of the command.</returns>
    public async Task<CommandResult> ToggleAsync()
    {
        ProcessResult query = await runner.RunAsync("xset", ["q"]);
        if (!query.Success)
        {
            return new CommandResult(false, "xset failed: " + query.Error.Trim());
        }
        bool isOn = !query.Output.Contains("Monitor is Off", StringComparison.Ordinal);
        string state = isOn ? "off" : "on";
        ProcessResult toggle = await runner.RunAsync("xset", ["dpms", "force", state]);
        return toggle.Success ? new CommandResult(true, "Screen turned " + state) : new CommandResult(false, "xset failed: " + toggle.Error.Trim());
    }
}