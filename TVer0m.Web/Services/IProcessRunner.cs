namespace TVer0m.Web.Services;

/// <summary>
/// Runs external programs without a shell.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Runs a program with the given arguments and captures its output.
    /// </summary>
    /// <param name="fileName">The program to run.</param>
    /// <param name="arguments">The arguments, passed individually and never through a shell.</param>
    /// <param name="cancellationToken">A token to cancel the run.</param>
    /// <returns>The captured result.</returns>
    Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default);
}