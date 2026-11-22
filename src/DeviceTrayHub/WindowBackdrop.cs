using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace German.DeviceTrayHub;

internal static class WindowBackdrop
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmSystemBackdropTransientWindow = 3;

    public static void Apply(Window? window)
    {
        if (window is null || !window.IsLoaded && !new WindowInteropHelper(window).Handle.Equals(IntPtr.Zero))
            return;

        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero) return;

        var isDark = false;
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            isDark = Convert.ToInt32(key?.GetValue("AppsUseLightTheme", 1)) == 0;
        }
        catch { }

        var darkMode = isDark ? 1 : 0;
        _ = DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode,
            ref darkMode, sizeof(int));

        var backdrop = DwmSystemBackdropTransientWindow;
        _ = DwmSetWindowAttribute(handle, DwmwaSystemBackdropType,
            ref backdrop, sizeof(int));
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attribute, ref int value, int valueSize);
}