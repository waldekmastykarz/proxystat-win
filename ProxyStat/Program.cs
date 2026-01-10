namespace ProxyStat;

/// <summary>
/// ProxyStat - Windows System Tray Proxy Status Indicator
/// Shows whether HTTPS proxy is enabled in Windows settings
/// </summary>
public static class Program
{
    [STAThread]
    public static void Main()
    {
        using var trayIcon = new NativeTrayIcon();
        trayIcon.Run();
    }
}
