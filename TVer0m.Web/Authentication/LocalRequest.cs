using System.Net;

namespace TVer0m.Web.Authentication;

/// <summary>
/// Works out whether a request comes from the iMac itself.
/// </summary>
public static class LocalRequest
{
    /// <summary>
    /// Checks whether the connection came from the loopback address.
    /// </summary>
    /// <param name="context">The current request.</param>
    /// <returns>True when the request came from this machine.</returns>
    public static bool IsLocal(HttpContext context)
    {
        IPAddress? address = context.Connection.RemoteIpAddress;
        return address is not null && IPAddress.IsLoopback(address);
    }
}