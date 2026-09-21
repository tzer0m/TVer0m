namespace TVer0m.Web.Services;

/// <summary>
/// The outcome of an API command, returned to the caller as JSON.
/// </summary>
/// <param name="Success">Whether the command worked.</param>
/// <param name="Message">A short description of what happened.</param>
public record CommandResult(bool Success, string Message);