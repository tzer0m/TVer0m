namespace TVer0m.Web.Detection;

/// <summary>
/// Decides from the User-Agent and an optional override where a visitor should go.
/// </summary>
public class DeviceDetector
{
    /// <summary>
    /// User-Agent fragments that identify the kiosk browser.
    /// </summary>
    private static readonly string[] KioskMarkers = ["SMART-TV", "Tizen"];

    /// <summary>
    /// User-Agent fragments that identify phones and tablets.
    /// </summary>
    private static readonly string[] MobileMarkers = ["Android", "iPhone", "iPad", "Mobile"];

    /// <summary>
    /// Picks the target for a visitor, honouring the mode override first.
    /// </summary>
    /// <param name="userAgent">The User-Agent header, if any.</param>
    /// <param name="mode">The value of the mode query parameter, if any.</param>
    /// <returns>The target the visitor should be sent to.</returns>
    public DeviceTarget Detect(string? userAgent, string? mode)
    {
        if (string.Equals(mode, "host", StringComparison.OrdinalIgnoreCase))
        {
            return DeviceTarget.Host;
        }
        if (string.Equals(mode, "remote", StringComparison.OrdinalIgnoreCase))
        {
            return DeviceTarget.Remote;
        }
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return DeviceTarget.Ambiguous;
        }
        if (ContainsAny(userAgent, KioskMarkers))
        {
            return DeviceTarget.Host;
        }
        if (ContainsAny(userAgent, MobileMarkers))
        {
            return DeviceTarget.Remote;
        }
        return DeviceTarget.Ambiguous;
    }

    /// <summary>
    /// Checks whether the User-Agent contains any of the markers.
    /// </summary>
    /// <param name="userAgent">The User-Agent header.</param>
    /// <param name="markers">The fragments to look for.</param>
    /// <returns>True if any marker is found, ignoring case.</returns>
    private static bool ContainsAny(string userAgent, string[] markers)
    {
        foreach (string marker in markers)
        {
            if (userAgent.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}