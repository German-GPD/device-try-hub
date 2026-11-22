using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace German.DeviceTrayHub;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _pollTimer;
    private bool _refreshInProgress;

    public MainWindow()
    {
        InitializeComponent();
        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _pollTimer.Tick += async (_, _) => await RefreshDevicesAsync();
        Loaded += async (_, _) =>
        {
            PositionNearTray();
            await RefreshDevicesAsync();
            _pollTimer.Start();
        };
        Deactivated += (_, _) => Hide();
    }

    private void PositionNearTray()
    {
        var work = SystemParameters.WorkArea;
        Left = work.Right - Width - 12;
        Top = work.Bottom - Height - 12;
    }

    private async Task RefreshDevicesAsync()
    {
        if (_refreshInProgress) return;
        _refreshInProgress = true;

        try
        {
            var devices = await Task.Run(DeviceMonitor.ReadCurrent);
            DeviceList.Children.Clear();

            foreach (var device in devices)
            {
                var card = new Border
                {
                    Background = (Brush)Application.Current.Resources["SurfaceBrush"],
                    BorderBrush = (Brush)Application.Current.Resources["BorderBrush"],
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(9),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 9)
                };

                var panel = new StackPanel();
                var header = new DockPanel();
                var name = new TextBlock
                {
                    Text = device.Name,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold
                };
                var battery = new TextBlock
                {
                    Text = device.Battery,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)Application.Current.Resources["AccentBrush"]
                };
                DockPanel.SetDock(battery, Dock.Right);
                header.Children.Add(battery);
                header.Children.Add(name);
                panel.Children.Add(header);
                panel.Children.Add(new TextBlock
                {
                    Text = device.Connection + "  ·  " + device.Charging,
                    FontSize = 11,
                    Margin = new Thickness(0, 6, 0, 0),
                    Foreground = (Brush)Application.Current.Resources["MutedBrush"]
                });
                panel.Children.Add(new TextBlock
                {
                    Text = device.Detail,
                    FontSize = 10,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 0),
                    Foreground = (Brush)Application.Current.Resources["MutedBrush"]
                });
                card.Child = panel;
                DeviceList.Children.Add(card);
            }
        }
        finally
        {
            _refreshInProgress = false;
        }
    }

    private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();

    private void StartupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            var exe = Environment.ProcessPath;
            if (key is null || string.IsNullOrWhiteSpace(exe)) return;

            var current = key.GetValue("GermanDeviceTrayHub") as string;
            if (current is null)
                key.SetValue("GermanDeviceTrayHub", $"\"{exe}\"");
            else
                key.DeleteValue("GermanDeviceTrayHub", false);

            MessageBox.Show(current is null
                ? "Автозапуск включён для текущего пользователя."
                : "Автозапуск выключен.",
                "Device Tray Hub", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось изменить автозапуск: {ex.Message}",
                "Device Tray Hub", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}