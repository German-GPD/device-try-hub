using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace German.DeviceTrayHub;

internal static class TrayIconFactory
{
    public static Icon Create()
    {
        using var bitmap = new Bitmap(32, 32);
        using (var graphics = Graphics.FromImage(bitmap))
        using (var pen = new Pen(Color.White, 2.2f))
        using (var dot = new SolidBrush(Color.White))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;

            // Minimal white device-list glyph, designed to remain clear at 16x16.
            graphics.DrawRoundedRectangle(pen, new Rectangle(5, 5, 22, 22), 4);
            graphics.FillEllipse(dot, 10, 11, 3, 3);
            graphics.FillEllipse(dot, 10, 18, 3, 3);
            graphics.DrawLine(pen, 16, 12.5f, 23, 12.5f);
            graphics.DrawLine(pen, 16, 19.5f, 23, 19.5f);
        }

        var handle = bitmap.GetHicon();
        try
        {
            using var temporary = Icon.FromHandle(handle);
            return (Icon)temporary.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr handle);
}

internal static class GraphicsExtensions
{
    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radius)
    {
        using var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        graphics.DrawPath(pen, path);
    }
}