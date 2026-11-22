using System.Diagnostics;
using System.Globalization;

namespace German.DeviceTrayHub;

internal static class Ds4WindowsAdapter
{
    private const string ProcessName = "DS4Windows";
    private const int ControllerSlot = 1;
    private const int QueryTimeoutMilliseconds = 3000;

    // Query on each tray activation so the displayed value is a fresh IPC response.
    public static DeviceStatus ReadStatus() => ReadStatusCore();

    private static DeviceStatus ReadStatusCore()
    {
        var executable = FindRunningExecutable();
        if (executable is null)
        {
            return new DeviceStatus(
                "Sony DualShock 4",
                "DS4Windows не запущен",
                "Недоступно",
                "Неизвестно",
                "Запусти DS4Windows — приложение читает состояние через его штатный IPC.");
        }

        var batteryText = Query(executable, "Battery");
        if (string.IsNullOrWhiteSpace(batteryText))
        {
            return new DeviceStatus(
                "Sony DualShock 4",
                "Контроллер не найден в слоте 1",
                "Недоступно",
                "Неизвестно",
                "DS4Windows работает, но не вернул батарею для первого слота.");
        }

        var normalizedBattery = batteryText.Trim().TrimEnd('%').Trim();
        if (!int.TryParse(normalizedBattery, NumberStyles.Integer, CultureInfo.InvariantCulture, out var battery) ||
            battery is < 0 or > 100)
        {
            return new DeviceStatus(
                "Sony DualShock 4",
                "Получен неизвестный ответ",
                "Недоступно",
                "Неизвестно",
                $"DS4Windows вернул непонятное значение батареи: {batteryText}");
        }

        var chargingText = Query(executable, "Charging");
        var connectionText = Query(executable, "ConnType");

        var connection = connectionText.Trim().ToUpperInvariant() switch
        {
            "BT" => "Подключён · Bluetooth",
            "USB" => "Подключён · USB",
            _ => null
        };

        if (connection is null)
        {
            return new DeviceStatus(
                "Sony DualShock 4",
                "Контроллер не подтверждён",
                "Недоступно",
                "Неизвестно",
                "DS4Windows не подтвердил активное USB- или Bluetooth-подключение.");
        }

        var charging = chargingText.Trim().ToLowerInvariant() switch
        {
            "true" or "1" => "Заряжается",
            "false" or "0" => "Не заряжается",
            _ => "Неизвестно"
        };

        return new DeviceStatus(
            "Sony DualShock 4",
            connection,
            $"{battery}%",
            charging,
            "Данные получены через штатный IPC DS4Windows (слот 1).");
    }

    private static string? FindRunningExecutable()
    {
        foreach (var process in Process.GetProcessesByName(ProcessName))
        {
            using (process)
            {
                try
                {
                    var path = process.MainModule?.FileName;
                    if (!string.IsNullOrWhiteSpace(path)) return path;
                }
                catch
                {
                    // A protected or elevated process may hide its module path.
                }
            }
        }

        return null;
    }

    private static string Query(string executable, string property)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("-command");
            startInfo.ArgumentList.Add($"Query.{ControllerSlot}.{property}");

            using var process = Process.Start(startInfo);
            if (process is null) return string.Empty;

            // Drain stdout concurrently; waiting first can block a child that fills the pipe.
            var outputTask = process.StandardOutput.ReadToEndAsync();
            if (!process.WaitForExit(QueryTimeoutMilliseconds))
            {
                process.Kill(entireProcessTree: true);
                return string.Empty;
            }

            return outputTask.GetAwaiter().GetResult().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }
}