using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace German.DeviceTrayHub;

internal static class ThemeService
{
    public static void Apply()
    {
        var resources = Application.Current.Resources;
        var isLight = true;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            isLight = Convert.ToInt32(key?.GetValue("AppsUseLightTheme", 1)) != 0;
        }
        catch { /* Keep the accessible light fallback. */ }

        var accent = ReadAccent();
        resources["WindowBrush"] = Brush(isLight ? "#FFF4F5F7" : "#FF17191D");
        resources["SurfaceBrush"] = Brush(isLight ? "#FFFFFFFF" : "#FF22252A");
        resources["TextBrush"] = Brush(isLight ? "#FF202329" : "#FFF1F2F4");
        resources["MutedBrush"] = Brush(isLight ? "#FF737983" : "#FFA5ABB4");
        resources["BorderBrush"] = Brush(isLight ? "#FFE3E6EA" : "#FF353A42");
        resources["AccentBrush"] = new SolidColorBrush(accent);
    }

    private static SolidColorBrush Brush(string value) =>
        new((Color)ColorConverter.ConvertFromString(value));

    private static Color ReadAccent()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            if (key?.GetValue("AccentColor") is int argb)
            {
                // Windows stores this DWORD as ABGR; WPF expects ARGB.
                var r = (byte)(argb & 0xFF);
                var g = (byte)((argb >> 8) & 0xFF);
                var b = (byte)((argb >> 16) & 0xFF);
                return Color.FromRgb(r, g, b);
            }
        }
        catch { }
        return Color.FromRgb(46, 139, 114);
    }
}