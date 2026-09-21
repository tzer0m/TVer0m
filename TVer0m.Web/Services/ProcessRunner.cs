using System.Diagnostics;

namespace TVer0m.Web.Services;

/// <summary>
/// Runs external programs with ProcessStartInfo and ArgumentList, never through a shell.
/// </summary>
/// <param name="logger">The logger.</param>
public class ProcessRunner(ILogger<ProcessRunner> logger) : IProcessRunner
{
    /// <summary>
    /// The X display used when DISPLAY is not set.
    /// </summary>
    private const string DefaultDisplay = ":0";

    /// <summary>
    /// The X authority file used when XAUTHORITY is not set.
    /// </summary>
    private const string DefaultXAuthority = "/home/tzer0m/.Xauthority";

    /// <summary>
    /// How long a process may run before it is killed.
    /// </summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Runs a program with the given arguments and captures its output.
    /// </summary>
    /// <param name="fileName">The program to run.</param>
    /// <param name="arguments">The arguments, passed individually and never through a shell.</param>
    /// <param name="cancellationToken">A token to cancel the run.</param>
    /// <returns>The captured result.</returns>
    public async Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
    {
        ProcessStartInfo startInfo = new() { FileName = fileName, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
        {
            startInfo.Environment["DISPLAY"] = DefaultDisplay;
        }
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XAUTHORITY")))
        {
            startInfo.Environment["XAUTHORITY"] = DefaultXAuthority;
        }
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Running: {FileName} {Arguments}", fileName, string.Join(' ', arguments));
        }
        using Process process = new() { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not start {FileName}", fileName);
            return new ProcessResult(-1, string.Empty, exception.Message);
        }
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(Timeout);
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
        Task<string> errorTask = process.StandardError.ReadToEndAsync(timeoutSource.Token);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("{FileName} timed out or was cancelled", fileName);
            process.Kill(true);
            return new ProcessResult(-1, string.Empty, "Timed out or cancelled");
        }
        return new ProcessResult(process.ExitCode, await outputTask, await errorTask);
    }
}