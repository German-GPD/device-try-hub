using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using WpfApplication = System.Windows.Application;
using MediaBrush = System.Windows.Media.Brush;
using WpfProgressBar = System.Windows.Controls.ProgressBar;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;

namespace German.DeviceTrayHub;

public partial class MainWindow : Window
{
    private bool _refreshInProgress;
    private bool _allowClose;
    private bool _hasRefreshed;

    public MainWindow()
    {
        InitializeComponent();
        IsVisibleChanged += async (_, _) =>
        {
            if (!IsVisible) return;
            PositionNearTray();
            await RefreshDevicesAsync();
        };
        Loaded += (_, _) => UpdateStartupButtonText();
        UpdateStartupButtonText();
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
        RefreshStatus.Text = "Обновляю…";

        try
        {
            var devices = await Task.Run(DeviceMonitor.ReadCurrent);
            DeviceList.Children.Clear();

            foreach (var device in devices)
            {
                var card = new Border
                {
                    Background = (MediaBrush)WpfApplication.Current.Resources["SurfaceBrush"],
                    BorderBrush = (MediaBrush)WpfApplication.Current.Resources["BorderBrush"],
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
                    Foreground = (MediaBrush)WpfApplication.Current.Resources[
                        device.Battery == "Недоступно" ? "MutedBrush" : "AccentBrush"]
                };
                DockPanel.SetDock(battery, Dock.Right);
                header.Children.Add(battery);
                header.Children.Add(name);
                panel.Children.Add(header);
                if (int.TryParse(device.Battery.TrimEnd('%'), out var batteryPercent) &&
                    batteryPercent is >= 0 and <= 100)
                {
                    panel.Children.Add(new WpfProgressBar
                    {
                        Minimum = 0,
                        Maximum = 100,
                        Value = batteryPercent,
                        Height = 4,
                        Margin = new Thickness(0, 8, 0, 1),
                        Foreground = (MediaBrush)WpfApplication.Current.Resources["AccentBrush"],
                        Background = (MediaBrush)WpfApplication.Current.Resources["BorderBrush"],
                        BorderThickness = new Thickness(0)
                    });
                }

                panel.Children.Add(new TextBlock
                {
                    Text = device.Connection + "  ·  " + device.Charging,
                    FontSize = 11,
                    Margin = new Thickness(0, 6, 0, 0),
                    Foreground = (MediaBrush)WpfApplication.Current.Resources["MutedBrush"]
                });
                panel.Children.Add(new TextBlock
                {
                    Text = device.Detail,
                    FontSize = 10,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 0),
                    Foreground = (MediaBrush)WpfApplication.Current.Resources["MutedBrush"]
                });
                card.Child = panel;
                DeviceList.Children.Add(card);
            }

            _hasRefreshed = true;
            RefreshStatus.Text = "Обновлено";
        }
        catch (Exception ex)
        {
            RefreshStatus.Text = "Ошибка обновления";
            if (!_hasRefreshed)
            {
                DeviceList.Children.Clear();
                DeviceList.Children.Add(new TextBlock
                {
                    Text = "Не удалось получить состояние устройств: " + ex.Message,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = (MediaBrush)WpfApplication.Current.Resources["MutedBrush"]
                });
            }
        }
        finally
        {
            _refreshInProgress = false;
        }
    }

    public void AllowClose() => _allowClose = true;

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();

    private void UpdateStartupButtonText()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            var enabled = key?.GetValue("GermanDeviceTrayHub") is string value && !string.IsNullOrWhiteSpace(value);
            StartupButton.Content = enabled ? "Автозапуск: включён" : "Автозапуск: выключен";
        }
        catch
        {
            StartupButton.Content = "Автозапуск: недоступен";
        }
    }

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

            UpdateStartupButtonText();
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show($"Не удалось изменить автозапуск: {ex.Message}",
                "Device Tray Hub", WpfMessageBoxButton.OK, WpfMessageBoxImage.Warning);
        }
    }
}