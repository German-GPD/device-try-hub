using System.Diagnostics;
using System.Globalization;

namespace German.DeviceTrayHub;

internal static class Ds4WindowsAdapter
{
    private const string ProcessName = "DS4Windows";
    private const int FirstControllerSlot = 1;
    private const int LastControllerSlot = 4;
    private const int QueryTimeoutMilliseconds = 1500;

    // DS4Windows can assign a controller to any of its four slots. Find the
    // first slot that currently reports a real connection instead of assuming slot 1.
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

        for (var slot = FirstControllerSlot; slot <= LastControllerSlot; slot++)
        {
            var connectionText = Query(executable, slot, "ConnType");
            var connection = connectionText.Trim().ToUpperInvariant() switch
            {
                "BT" => "Подключён · Bluetooth",
                "USB" => "Подключён · USB",
                _ => null
            };

            if (connection is null) continue;

            var batteryText = Query(executable, slot, "Battery");
            var normalizedBattery = batteryText.Trim().TrimEnd('%').Trim();
            if (!int.TryParse(normalizedBattery, NumberStyles.Integer, CultureInfo.InvariantCulture, out var battery) ||
                battery is < 0 or > 100)
            {
                return new DeviceStatus(
                    "Sony DualShock 4",
                    connection,
                    "Недоступно",
                    "Неизвестно",
                    $"DS4Windows подтвердил контроллер в слоте {slot}, но не вернул корректный заряд.");
            }

            var chargingText = Query(executable, slot, "Charging");
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
                $"Данные получены через штатный IPC DS4Windows (слот {slot}).");
        }

        return new DeviceStatus(
            "Sony DualShock 4",
            "Контроллер не подключён в DS4Windows",
            "Недоступно",
            "Неизвестно",
            "Проверены слоты 1–4; активного USB- или Bluetooth-контроллера не найдено.");
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

    private static string Query(string executable, int slot, string property)
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
            startInfo.ArgumentList.Add($"Query.{slot}.{property}");

            using var process = Process.Start(startInfo);
            if (process is null) return string.Empty;

            // Drain stdout concurrently so a child cannot block on a full pipe.
            var outputTask = process.StandardOutput.ReadToEndAsync();
            if (!process.WaitForExit(QueryTimeoutMilliseconds))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
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
