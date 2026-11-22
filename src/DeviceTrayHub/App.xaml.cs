using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace German.DeviceTrayHub;

public partial class App : Application
{
    private Mutex? _mutex;
    private NotifyIcon? _trayIcon;
    private MainWindow? _window;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _mutex = new Mutex(true, @"Local\GermanDeviceTrayHub", out var createdNew);
        if (!createdNew)
        {
            Shutdown();
            return;
        }

        ThemeService.Apply();
        Microsoft.Win32.SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

        _window = new MainWindow();
        _window.Show();
        _window.Hide();

        var menu = new ContextMenuStrip();
        menu.Items.Add("Открыть Device Tray Hub", null, (_, _) => ShowWindow());
        menu.Items.Add("Выход", null, (_, _) => Shutdown());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "German'$ Device Tray Hub",
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.MouseClick += (_, args) =>
        {
            if (args.Button == MouseButtons.Left) ShowWindow();
        };
    }

    private void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        Dispatcher.Invoke(ThemeService.Apply);
    }

    private void ShowWindow()
    {
        if (_window is null) return;
        _window.Show();
        _window.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}