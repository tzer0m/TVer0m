using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace TVer0m.Web.Services;

/// <summary>
/// Sends the kiosk browser home, using the Chromium DevTools port first and xdotool as a fallback.
/// </summary>
/// <param name="client">The HTTP client used to reach the DevTools endpoint.</param>
/// <param name="runner">The process runner.</param>
/// <param name="logger">The logger.</param>
public class BrowserController(HttpClient client, IProcessRunner runner, ILogger<BrowserController> logger) : IBrowserController
{
    /// <summary>
    /// The page the kiosk is sent back to.
    /// </summary>
    private const string HomeUrl = "http://localhost:8090/host";

    /// <summary>
    /// The Chromium DevTools endpoint that lists open pages.
    /// </summary>
    private const string DevToolsListUrl = "http://127.0.0.1:9222/json";

    /// <summary>
    /// How long the whole DevTools attempt may take.
    /// </summary>
    private static readonly TimeSpan DevToolsTimeout = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Navigates the browser back to the host page.
    /// </summary>
    /// <returns>The outcome of the command.</returns>
    public async Task<CommandResult> HomeAsync()
    {
        if (await TryNavigateWithDevToolsAsync())
        {
            return new CommandResult(true, "Navigated home via DevTools");
        }
        logger.LogWarning("DevTools navigation failed, falling back to xdotool");
        return await NavigateWithXdotoolAsync();
    }

    /// <summary>
    /// Sends Page.navigate to the active page over the DevTools WebSocket.
    /// </summary>
    /// <returns>True if Chromium accepted the navigation.</returns>
    private async Task<bool> TryNavigateWithDevToolsAsync()
    {
        try
        {
            using CancellationTokenSource timeout = new(DevToolsTimeout);
            string listJson = await client.GetStringAsync(DevToolsListUrl, timeout.Token);
            string? socketUrl = FindPageSocketUrl(listJson);
            if (socketUrl is null)
            {
                return false;
            }
            using ClientWebSocket socket = new();
            await socket.ConnectAsync(new Uri(socketUrl), timeout.Token);
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { id = 1, method = "Page.navigate", @params = new { url = HomeUrl } }));
            await socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, timeout.Token);
            return await WaitForReplyAsync(socket, timeout.Token);
        }
        catch (Exception exception) when (exception is HttpRequestException or WebSocketException or OperationCanceledException or JsonException or UriFormatException)
        {
            logger.LogWarning(exception, "DevTools navigation failed");
            return false;
        }
    }

    /// <summary>
    /// Finds the WebSocket URL of the first real page in the DevTools list.
    /// </summary>
    /// <param name="listJson">The JSON returned by the DevTools list endpoint.</param>
    /// <returns>The WebSocket URL, or null if there is no suitable page.</returns>
    private static string? FindPageSocketUrl(string listJson)
    {
        using JsonDocument document = JsonDocument.Parse(listJson);
        foreach (JsonElement target in document.RootElement.EnumerateArray())
        {
            string? url = GetString(target, "url");
            if (GetString(target, "type") != "page" || url is null || url.StartsWith("devtools://", StringComparison.Ordinal))
            {
                continue;
            }
            string? socketUrl = GetString(target, "webSocketDebuggerUrl");
            if (socketUrl is not null)
            {
                return socketUrl;
            }
        }
        return null;
    }

    /// <summary>
    /// Reads a string property from a JSON object if it exists.
    /// </summary>
    /// <param name="element">The JSON object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or null if the property is missing.</returns>
    private static string? GetString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out JsonElement value) ? value.GetString() : null;
    }

    /// <summary>
    /// Waits for the reply to command 1, skipping any DevTools events sent first.
    /// </summary>
    /// <param name="socket">The open DevTools WebSocket.</param>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>True if the reply arrived without an error.</returns>
    private static async Task<bool> WaitForReplyAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[16384];
        while (socket.State == WebSocketState.Open)
        {
            using MemoryStream message = new();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                message.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                return false;
            }
            using JsonDocument reply = JsonDocument.Parse(message.ToArray());
            if (reply.RootElement.TryGetProperty("id", out JsonElement id) && id.GetInt32() == 1)
            {
                return !reply.RootElement.TryGetProperty("error", out JsonElement _);
            }
        }
        return false;
    }

    /// <summary>
    /// Falls back to typing the home URL into the address bar with xdotool.
    /// </summary>
    /// <returns>The outcome of the command.</returns>
    private async Task<CommandResult> NavigateWithXdotoolAsync()
    {
        IReadOnlyList<string>[] steps = [["key", "--clearmodifiers", "ctrl+l"], ["type", "--clearmodifiers", "--", HomeUrl], ["key", "--clearmodifiers", "Return"]];
        foreach (IReadOnlyList<string> step in steps)
        {
            ProcessResult result = await runner.RunAsync("xdotool", step);
            if (!result.Success)
            {
                return new CommandResult(false, "xdotool failed: " + result.Error.Trim());
            }
            await Task.Delay(150);
        }
        return new CommandResult(true, "Navigated home via xdotool");
    }
}