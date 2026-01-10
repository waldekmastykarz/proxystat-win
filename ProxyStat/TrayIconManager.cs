using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace ProxyStat;

/// <summary>
/// Manages the system tray icon and context menu
/// Equivalent to the macOS NSStatusItem functionality
/// </summary>
public class TrayIconManager : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private RegistryWatcher? _registryWatcher;
    private bool _disposed;

    private Icon? _activeIcon;
    private Icon? _inactiveIcon;

    public void Initialize()
    {
        LoadIcons();
        CreateNotifyIcon();
        StartWatching();
        UpdateProxyStatus();
    }

    private void LoadIcons()
    {
        // Load icons from embedded resources (copied from macOS project)
        _activeIcon = LoadIconFromResource("ProxyStat.Resources.ProxyWhiteActive.png");
        _inactiveIcon = LoadIconFromResource("ProxyStat.Resources.ProxyInactive.png");
    }

    private static Icon? LoadIconFromResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;
        
        using var bitmap = new Bitmap(stream);
        return Icon.FromHandle(bitmap.GetHicon());
    }

    private void CreateNotifyIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = _inactiveIcon,
            Visible = true,
            Text = "ProxyStat - Checking proxy status..."
        };

        // Create context menu (equivalent to NSMenu)
        var contextMenu = new ContextMenuStrip();
        
        // "System Proxy Settings..." menu item
        var proxySettingsItem = new ToolStripMenuItem("System Proxy Settings...");
        proxySettingsItem.Click += OnOpenProxySettings;
        proxySettingsItem.ShortcutKeyDisplayString = "Ctrl+,";
        contextMenu.Items.Add(proxySettingsItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        // "Quit" menu item
        var quitItem = new ToolStripMenuItem("Quit");
        quitItem.Click += OnQuit;
        quitItem.ShortcutKeyDisplayString = "Ctrl+Q";
        contextMenu.Items.Add(quitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        
        // Double-click opens proxy settings
        _notifyIcon.DoubleClick += OnOpenProxySettings;
    }

    private void StartWatching()
    {
        // Watch registry for proxy setting changes (event-driven, no polling)
        _registryWatcher = new RegistryWatcher(UpdateProxyStatus);
        _registryWatcher.Start();
    }

    private void UpdateProxyStatus()
    {
        var proxyEnabled = ProxyStatus.IsProxyEnabled();
        var proxyServer = ProxyStatus.GetProxyServer();

        if (_notifyIcon == null) return;

        if (proxyEnabled)
        {
            _notifyIcon.Icon = _activeIcon;
            _notifyIcon.Text = $"ProxyStat - Proxy Enabled\n{proxyServer ?? "No server configured"}";
        }
        else
        {
            _notifyIcon.Icon = _inactiveIcon;
            _notifyIcon.Text = "ProxyStat - Proxy Disabled";
        }
    }

    private void OnOpenProxySettings(object? sender, EventArgs e)
    {
        // Open Windows Proxy Settings
        // ms-settings:network-proxy is the modern Settings app URI
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:network-proxy",
                UseShellExecute = true
            });
        }
        catch
        {
            // Fallback to Control Panel Internet Options
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "control.exe",
                    Arguments = "inetcpl.cpl,,4",  // Open Internet Options, Connections tab
                    UseShellExecute = true
                });
            }
            catch
            {
                // Silently fail if neither works
            }
        }
    }

    private void OnQuit(object? sender, EventArgs e)
    {
        Application.Current.Shutdown();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _registryWatcher?.Dispose();
        
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        _activeIcon?.Dispose();
        _inactiveIcon?.Dispose();
    }
}
