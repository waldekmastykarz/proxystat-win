using System.Reflection;
using System.Runtime.InteropServices;
using static ProxyStat.NativeMethods;

namespace ProxyStat;

/// <summary>
/// Pure Win32 tray icon implementation - no external dependencies
/// </summary>
internal sealed class NativeTrayIcon : IDisposable
{
    private const int IDM_SETTINGS = 1001;
    private const int IDM_DISABLE = 1002;
    private const int IDM_QUIT = 1003;

    private IntPtr _hwnd;
    private IntPtr _activeIcon;
    private IntPtr _inactiveIcon;
    private NOTIFYICONDATA _nid;
    private readonly WndProcDelegate _wndProcDelegate;
    private RegistryWatcher? _registryWatcher;
    private bool _proxyEnabled;
    private GCHandle _wndProcHandle;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public NativeTrayIcon()
    {
        _wndProcDelegate = WndProc;
        _wndProcHandle = GCHandle.Alloc(_wndProcDelegate);
    }

    public void Run()
    {
        var hInstance = GetModuleHandle(null);

        var wc = new WNDCLASS
        {
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = hInstance,
            lpszClassName = "ProxyStatTrayClass",
            lpszMenuName = null
        };
        RegisterClass(ref wc);

        _hwnd = CreateWindowEx(0, "ProxyStatTrayClass", "ProxyStat", 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

        LoadIcons();

        _nid = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = _inactiveIcon,
            szTip = "ProxyStat - Checking..."
        };
        Shell_NotifyIcon(NIM_ADD, ref _nid);

        _registryWatcher = new RegistryWatcher(UpdateStatus);
        _registryWatcher.Start();
        UpdateStatus();

        while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }

    private void LoadIcons()
    {
        _activeIcon = LoadIconFromResource("ProxyStat.Resources.ProxyActive.ico");
        _inactiveIcon = LoadIconFromResource("ProxyStat.Resources.ProxyInactive.ico");

        if (_activeIcon == IntPtr.Zero)
            _activeIcon = LoadIcon(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
        if (_inactiveIcon == IntPtr.Zero)
            _inactiveIcon = LoadIcon(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Resources preserved")]
    private static IntPtr LoadIconFromResource(string resourceName)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return IntPtr.Zero;

            // Write to temp file and load with LoadImage (handles PNG-compressed ICOs)
            var tempPath = Path.Combine(Path.GetTempPath(), $"proxystat_{Guid.NewGuid():N}.ico");
            try
            {
                using (var fs = File.Create(tempPath))
                {
                    stream.CopyTo(fs);
                }
                
                // LoadImage with LR_LOADFROMFILE handles modern ICO formats
                // Use 0,0 to load the best matching size for the system
                return LoadImage(IntPtr.Zero, tempPath, IMAGE_ICON, 0, 0, LR_LOADFROMFILE);
            }
            finally
            {
                try { File.Delete(tempPath); } catch { }
            }
        }
        catch { return IntPtr.Zero; }
    }

    private void UpdateStatus()
    {
        _proxyEnabled = ProxyStatus.IsProxyEnabled();
        var server = ProxyStatus.GetProxyServer();

        _nid.hIcon = _proxyEnabled ? _activeIcon : _inactiveIcon;
        _nid.szTip = _proxyEnabled
            ? $"ProxyStat - Proxy Enabled\n{server ?? "No server"}"
            : "ProxyStat - Proxy Disabled";

        Shell_NotifyIcon(NIM_MODIFY, ref _nid);
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_TRAYICON:
                var mouseMsg = (int)lParam;
                if (mouseMsg == WM_RBUTTONUP) ShowContextMenu();
                else if (mouseMsg == WM_LBUTTONDBLCLK) OpenProxySettings();
                return IntPtr.Zero;

            case WM_COMMAND:
                switch ((int)wParam)
                {
                    case IDM_SETTINGS: OpenProxySettings(); break;
                    case IDM_DISABLE: ProxyStatus.DisableProxy(); break;
                    case IDM_QUIT: Quit(); break;
                }
                return IntPtr.Zero;

            case WM_DESTROY:
                PostQuitMessage(0);
                return IntPtr.Zero;
        }
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void ShowContextMenu()
    {
        GetCursorPos(out POINT pt);
        SetForegroundWindow(_hwnd);

        var hMenu = CreatePopupMenu();
        AppendMenu(hMenu, MF_STRING, (IntPtr)IDM_SETTINGS, "System Proxy Settings...");
        AppendMenu(hMenu, MF_STRING, (IntPtr)IDM_DISABLE, "Disable Proxy");
        AppendMenu(hMenu, MF_SEPARATOR, IntPtr.Zero, null);
        AppendMenu(hMenu, MF_STRING, (IntPtr)IDM_QUIT, "Quit");

        var cmd = TrackPopupMenu(hMenu, TPM_RIGHTBUTTON | TPM_RETURNCMD, pt.X, pt.Y, 0, _hwnd, IntPtr.Zero);
        DestroyMenu(hMenu);

        if (cmd > 0)
        {
            switch (cmd)
            {
                case IDM_SETTINGS: OpenProxySettings(); break;
                case IDM_DISABLE: ProxyStatus.DisableProxy(); break;
                case IDM_QUIT: Quit(); break;
            }
        }
    }

    private static void OpenProxySettings() =>
        ShellExecute(IntPtr.Zero, "open", "ms-settings:network-proxy", null, null, 1);

    private void Quit() => DestroyWindow(_hwnd);

    public void Dispose()
    {
        _registryWatcher?.Dispose();
        Shell_NotifyIcon(NIM_DELETE, ref _nid);
        if (_wndProcHandle.IsAllocated) _wndProcHandle.Free();
    }
}
