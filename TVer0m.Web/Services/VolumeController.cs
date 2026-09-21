using System.Text.RegularExpressions;

namespace TVer0m.Web.Services;

/// <summary>
/// Changes the default audio output volume using pactl, never going above 100%.
/// </summary>
/// <param name="runner">The process runner.</param>
public partial class VolumeController(IProcessRunner runner) : IVolumeController
{
    /// <summary>
    /// The pactl name for the default output.
    /// </summary>
    private const string Sink = "@DEFAULT_SINK@";

    /// <summary>
    /// The accepted volume actions.
    /// </summary>
    private static readonly HashSet<string> Actions = ["up", "down", "mute"];

    /// <summary>
    /// Builds the pactl arguments for a volume action.
    /// </summary>
    /// <param name="action">The action from the request.</param>
    /// <param name="currentPercent">The current volume, used to stop up going past 100%.</param>
    /// <returns>The pactl arguments, or null if the action is not whitelisted.</returns>
    public static IReadOnlyList<string>? BuildArguments(string action, int currentPercent)
    {
        if (action == "up")
        {
            return ["set-sink-volume", Sink, currentPercent >= 95 ? "100%" : "+5%"];
        }
        if (action == "down")
        {
            return ["set-sink-volume", Sink, "-5%"];
        }
        if (action == "mute")
        {
            return ["set-sink-mute", Sink, "toggle"];
        }
        return null;
    }

    /// <summary>
    /// Reads the first volume percentage from pactl get-sink-volume output.
    /// </summary>
    /// <param name="output">The pactl output.</param>
    /// <returns>The percentage, or null if none was found.</returns>
    public static int? ParsePercent(string output)
    {
        Match match = PercentPattern().Match(output);
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }

    /// <summary>
    /// Matches the first percentage in pactl get-sink-volume output.
    /// </summary>
    /// <returns>The compiled regular expression.</returns>
    [GeneratedRegex(@"(\d+)%")]
    private static partial Regex PercentPattern();

    /// <summary>
    /// Checks whether a volume action is on the whitelist.
    /// </summary>
    /// <param name="action">The action from the request.</param>
    /// <returns>True if the action is one of up, down or mute.</returns>
    public bool IsValidAction(string action)
    {
        return Actions.Contains(action);
    }

    /// <summary>
    /// Applies a whitelisted volume action to the default sink.
    /// </summary>
    /// <param name="action">The action from the request.</param>
    /// <returns>The outcome of the command.</returns>
    public async Task<CommandResult> ApplyAsync(string action)
    {
        if (!IsValidAction(action))
        {
            return new CommandResult(false, "Unknown action");
        }
        int currentPercent = 0;
        if (action == "up")
        {
            ProcessResult current = await runner.RunAsync("pactl", ["get-sink-volume", Sink]);
            int? parsed = ParsePercent(current.Output);
            if (!current.Success || parsed is null)
            {
                return new CommandResult(false, "Could not read the current volume");
            }
            currentPercent = parsed.Value;
        }
        IReadOnlyList<string>? arguments = BuildArguments(action, currentPercent);
        if (arguments is null)
        {
            return new CommandResult(false, "Unknown action");
        }
        ProcessResult result = await runner.RunAsync("pactl", arguments);
        return result.Success ? new CommandResult(true, "Applied " + action) : new CommandResult(false, "pactl failed: " + result.Error.Trim());
    }
}