namespace TVer0m.Web.Services;

/// <summary>
/// Sends whitelisted key presses to the focused window using xdotool.
/// </summary>
/// <param name="runner">The process runner.</param>
public class InputInjector(IProcessRunner runner) : IInputInjector
{
    /// <summary>
    /// Maps the accepted key names to xdotool key names.
    /// </summary>
    private static readonly Dictionary<string, string> KeyMap = new(StringComparer.Ordinal) { ["up"] = "Up", ["down"] = "Down", ["left"] = "Left", ["right"] = "Right", ["ok"] = "Return", ["back"] = "Escape" };

    /// <summary>
    /// Builds the xdotool arguments for a key name.
    /// </summary>
    /// <param name="name">The key name from the request.</param>
    /// <returns>The xdotool arguments, or null if the name is not whitelisted.</returns>
    public static IReadOnlyList<string>? BuildArguments(string name)
    {
        if (!KeyMap.TryGetValue(name, out string? keySym))
        {
            return null;
        }
        return ["key", "--clearmodifiers", keySym];
    }

    /// <summary>
    /// Checks whether a key name is on the whitelist.
    /// </summary>
    /// <param name="name">The key name from the request.</param>
    /// <returns>True if the name is one of up, down, left, right, ok or back.</returns>
    public bool IsValidKey(string name)
    {
        return KeyMap.ContainsKey(name);
    }

    /// <summary>
    /// Sends a whitelisted key to the focused window.
    /// </summary>
    /// <param name="name">The key name from the request.</param>
    /// <returns>The outcome of the command.</returns>
    public async Task<CommandResult> SendKeyAsync(string name)
    {
        IReadOnlyList<string>? arguments = BuildArguments(name);
        if (arguments is null)
        {
            return new CommandResult(false, "Unknown key");
        }
        ProcessResult result = await runner.RunAsync("xdotool", arguments);
        return result.Success ? new CommandResult(true, "Sent " + name) : new CommandResult(false, "xdotool failed: " + result.Error.Trim());
    }
}