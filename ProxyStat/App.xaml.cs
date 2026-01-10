using System.Windows;

namespace ProxyStat;

/// <summary>
/// ProxyStat - Windows System Tray Proxy Status Indicator
/// Shows whether HTTPS proxy is enabled in Windows settings
/// </summary>
public partial class App : Application
{
    private TrayIconManager? _trayIconManager;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        _trayIconManager = new TrayIconManager();
        _trayIconManager.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconManager?.Dispose();
        base.OnExit(e);
    }
}
