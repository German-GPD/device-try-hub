using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using MediaColor = System.Windows.Media.Color;
using WpfApplication = System.Windows.Application;
using WpfSystemColors = System.Windows.SystemColors;
using MediaColorConverter = System.Windows.Media.ColorConverter;

namespace German.DeviceTrayHub;

internal static class ThemeService
{
    public static void Apply()
    {
        var resources = WpfApplication.Current.Resources;

        if (SystemParameters.HighContrast)
        {
            resources["WindowBrush"] = FrozenBrush(WpfSystemColors.WindowColor);
            resources["SurfaceBrush"] = FrozenBrush(WpfSystemColors.WindowColor);
            resources["TextBrush"] = FrozenBrush(WpfSystemColors.WindowTextColor);
            resources["MutedBrush"] = FrozenBrush(WpfSystemColors.GrayTextColor);
            resources["BorderBrush"] = FrozenBrush(WpfSystemColors.WindowTextColor);
            resources["AccentBrush"] = FrozenBrush(WpfSystemColors.HighlightColor);
            return;
        }

        var isLight = true;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            isLight = Convert.ToInt32(key?.GetValue("AppsUseLightTheme", 1)) != 0;
        }
        catch { }

        var accent = ReadAccent();
        resources["WindowBrush"] = Brush(isLight ? "#B8F7F8FA" : "#B8202328");
        resources["SurfaceBrush"] = Brush(isLight ? "#70FFFFFF" : "#702B3037");
        resources["TextBrush"] = Brush(isLight ? "#FF202329" : "#FFF1F2F4");
        resources["MutedBrush"] = Brush(isLight ? "#FF737983" : "#FFA5ABB4");
        resources["BorderBrush"] = Brush(isLight ? "#55FFFFFF" : "#554F555E");
        resources["AccentBrush"] = FrozenBrush(accent);
    }

    private static SolidColorBrush Brush(string value) =>
        FrozenBrush((MediaColor)MediaColorConverter.ConvertFromString(value));

    private static SolidColorBrush FrozenBrush(MediaColor color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static MediaColor ReadAccent()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            if (key?.GetValue("AccentColor") is int argb)
            {
                var r = (byte)(argb & 0xFF);
                var g = (byte)((argb >> 8) & 0xFF);
                var b = (byte)((argb >> 16) & 0xFF);
                return MediaColor.FromRgb(r, g, b);
            }
        }
        catch { }

        return MediaColor.FromRgb(57, 126, 104);
    }
}