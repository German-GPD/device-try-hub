using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace German.DeviceTrayHub;

public partial class App : Application
{
    private Mutex? _mutex;
    private EventWaitHandle? _showSignal;
    private NotifyIcon? _trayIcon;
    private Icon? _trayIconImage;
    private MainWindow? _window;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _showSignal = new EventWaitHandle(
            false,
            EventResetMode.AutoReset,
            @"Local\GermanDeviceTrayHub.Show");

        _mutex = new Mutex(true, @"Local\GermanDeviceTrayHub", out var createdNew);
        if (!createdNew)
        {
            try { _showSignal.Set(); } catch (ObjectDisposedException) { }
            _showSignal.Dispose();
            _showSignal = null;
            _mutex.Dispose();
            _mutex = null;
            Shutdown();
            return;
        }

        StartActivationListener();
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
            Icon = _trayIconImage = TrayIconFactory.Create(),
            Text = "German'$ Device Tray Hub",
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.MouseClick += (_, args) =>
        {
            if (args.Button == MouseButtons.Left) ShowWindow();
        };
    }

    private void StartActivationListener()
    {
        if (_showSignal is null) return;

        var signal = _showSignal;
        var listener = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    signal.WaitOne();
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                if (Dispatcher.HasShutdownStarted) return;
                Dispatcher.BeginInvoke(new Action(ShowWindow));
            }
        })
        {
            IsBackground = true,
            Name = "Device Tray Hub activation listener"
        };
        listener.Start();
    }

    private void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        if (!Dispatcher.HasShutdownStarted)
            Dispatcher.BeginInvoke(new Action(ThemeService.Apply));
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
        _trayIconImage?.Dispose();
        _showSignal?.Dispose();
        Microsoft.Win32.SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}