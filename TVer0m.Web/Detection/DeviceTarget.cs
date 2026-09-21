namespace TVer0m.Web.Detection;

/// <summary>
/// Where a visitor to the entry page should be sent.
/// </summary>
public enum DeviceTarget
{
    /// <summary>
    /// The User-Agent is not enough, so serve the interstitial page and let its script decide.
    /// </summary>
    Ambiguous,

    /// <summary>
    /// Send the visitor to the host page.
    /// </summary>
    Host,

    /// <summary>
    /// Send the visitor to the remote page.
    /// </summary>
    Remote
}