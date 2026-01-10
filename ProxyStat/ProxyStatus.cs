using Microsoft.Win32;

namespace ProxyStat;

/// <summary>
/// Detects Windows proxy settings from the registry
/// </summary>
public static class ProxyStatus
{
    private const string InternetSettingsKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    /// <summary>
    /// Checks if proxy is enabled in Windows Internet Settings
    /// </summary>
    /// <returns>True if proxy is enabled, false otherwise</returns>
    public static bool IsProxyEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(InternetSettingsKey);
            if (key == null)
            {
                return false;
            }

            // ProxyEnable: 1 = enabled, 0 = disabled
            var proxyEnable = key.GetValue("ProxyEnable");
            if (proxyEnable is int enableValue)
            {
                return enableValue == 1;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the current proxy server address if set
    /// </summary>
    /// <returns>Proxy server string or null if not set</returns>
    public static string? GetProxyServer()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(InternetSettingsKey);
            return key?.GetValue("ProxyServer") as string;
        }
        catch
        {
            return null;
        }
    }
}
