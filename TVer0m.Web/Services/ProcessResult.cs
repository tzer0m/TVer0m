namespace TVer0m.Web.Services;

/// <summary>
/// The captured result of running an external process.
/// </summary>
/// <param name="ExitCode">The process exit code, or -1 if it could not run.</param>
/// <param name="Output">Everything the process wrote to standard output.</param>
/// <param name="Error">Everything the process wrote to standard error.</param>
public record ProcessResult(int ExitCode, string Output, string Error)
{
    /// <summary>
    /// True when the process exited with code zero.
    /// </summary>
    public bool Success => ExitCode == 0;
}