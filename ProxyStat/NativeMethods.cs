using System.Runtime.InteropServices;

namespace ProxyStat;

/// <summary>
/// Native Win32 API declarations for shell notification icons
/// </summary>
internal static partial class NativeMethods
{
    public const int WM_USER = 0x0400;
    public const int WM_TRAYICON = WM_USER + 1;
    public const int WM_COMMAND = 0x0111;
    public const int WM_DESTROY = 0x0002;
    public const int WM_RBUTTONUP = 0x0205;
    public const int WM_LBUTTONDBLCLK = 0x0203;

    public const int NIM_ADD = 0x00000000;
    public const int NIM_MODIFY = 0x00000001;
    public const int NIM_DELETE = 0x00000002;

    public const int NIF_MESSAGE = 0x00000001;
    public const int NIF_ICON = 0x00000002;
    public const int NIF_TIP = 0x00000004;

    public const int TPM_RIGHTBUTTON = 0x0002;
    public const int TPM_RETURNCMD = 0x0100;

    public const int MF_STRING = 0x00000000;
    public const int MF_SEPARATOR = 0x00000800;

    public const int IDI_APPLICATION = 32512;
    public const int IMAGE_ICON = 1;
    public const int LR_LOADFROMFILE = 0x0010;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
    }

    // Use DllImport for complex struct marshalling (not supported by LibraryImport)
    [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIconW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

    [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "RegisterClassW", CharSet = CharSet.Unicode)]
    public static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", EntryPoint = "GetMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll", EntryPoint = "DispatchMessageW")]
    public static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll")]
    public static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", EntryPoint = "AppendMenuW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AppendMenu(IntPtr hMenu, int uFlags, IntPtr uIDNewItem, string? lpNewItem);

    [DllImport("user32.dll")]
    public static extern int TrackPopupMenu(IntPtr hMenu, int uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "LoadIconW")]
    public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("shell32.dll", EntryPoint = "ShellExecuteW", CharSet = CharSet.Unicode)]
    public static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string? lpParameters, string? lpDirectory, int nShowCmd);

    [DllImport("user32.dll", EntryPoint = "LoadImageW", CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadImage(IntPtr hInstance, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll")]
    public static extern int LookupIconIdFromDirectoryEx(IntPtr presbits, [MarshalAs(UnmanagedType.Bool)] bool fIcon, int cxDesired, int cyDesired, uint Flags);

    [DllImport("user32.dll")]
    public static extern IntPtr CreateIconFromResourceEx(IntPtr presbits, int dwResSize, [MarshalAs(UnmanagedType.Bool)] bool fIcon, int dwVer, int cxDesired, int cyDesired, uint Flags);
}
