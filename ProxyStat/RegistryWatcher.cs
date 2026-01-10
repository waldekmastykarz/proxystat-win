using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace ProxyStat;

/// <summary>
/// Watches for changes to Windows proxy settings in the registry.
/// Uses RegNotifyChangeKeyValue for efficient event-driven notifications
/// instead of polling.
/// </summary>
public class RegistryWatcher : IDisposable
{
    private const string InternetSettingsKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
    
    private readonly Action _onChanged;
    private readonly CancellationTokenSource _cts = new();
    private Task? _watchTask;
    private bool _disposed;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegNotifyChangeKeyValue(
        IntPtr hKey,
        bool bWatchSubtree,
        RegNotifyFilter dwNotifyFilter,
        IntPtr hEvent,
        bool fAsynchronous);

    [Flags]
    private enum RegNotifyFilter
    {
        Value = 0x00000004,  // REG_NOTIFY_CHANGE_LAST_SET - notify on value changes
    }

    public RegistryWatcher(Action onChanged)
    {
        _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
    }

    public void Start()
    {
        _watchTask = Task.Run(WatchLoop, _cts.Token);
    }

    private void WatchLoop()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(InternetSettingsKey);
                if (key == null) 
                {
                    // Key doesn't exist, wait and retry
                    Task.Delay(1000, _cts.Token).Wait();
                    continue;
                }

                // Get the native handle
                var handle = key.Handle.DangerousGetHandle();
                
                // Wait for registry changes (blocking call)
                // This is synchronous - it blocks until a change occurs
                var result = RegNotifyChangeKeyValue(
                    handle,
                    watchSubtree: false,
                    RegNotifyFilter.Value,
                    IntPtr.Zero,
                    fAsynchronous: false);

                if (result == 0 && !_cts.Token.IsCancellationRequested)
                {
                    // Registry changed, notify on UI thread
                    System.Windows.Application.Current?.Dispatcher.BeginInvoke(_onChanged);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // On error, wait a bit before retrying
                try { Task.Delay(1000, _cts.Token).Wait(); } catch { break; }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();
        
        try
        {
            _watchTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch { }
        
        _cts.Dispose();
    }
}
