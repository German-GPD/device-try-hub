using HidSharp;

namespace German.DeviceTrayHub;

internal static class R1BatteryAdapter
{
    private const int VendorId = 0x1D57;
    private const int WirelessProductId = 0xFA60;
    private const string InterfaceMarker = "&mi_02";
    private const int ReadTimeoutMilliseconds = 1200;

    private const byte BatteryReportId = 0x03;
    private const byte R1DeviceId = 0x10;
    private const byte BatteryReady = 0x40;
    private static DeviceStatus? _lastKnownStatus;

    public static DeviceStatus ReadStatus()
    {
        try
        {
            var device = DeviceList.Local
                .GetHidDevices(VendorId, WirelessProductId)
                .FirstOrDefault(candidate =>
                    candidate.DevicePath.Contains(InterfaceMarker, StringComparison.OrdinalIgnoreCase));

            if (device is null)
            {
                _lastKnownStatus = null;
                return new DeviceStatus(
                    "Attack Shark R1",
                    "Приёмник не обнаружен",
                    "Недоступно",
                    "Неизвестно",
                    "USB HID-интерфейс приёмника R1 не найден.");
            }

            if (device.GetMaxInputReportLength() < 5)
            {
                return new DeviceStatus(
                    "Attack Shark R1",
                    "Приёмник найден",
                    "Недоступно",
                    "Неизвестно",
                    "Размер HID-отчёта не соответствует ожидаемому формату.");
            }

            using var stream = device.Open();
            stream.ReadTimeout = ReadTimeoutMilliseconds;
            var report = stream.Read();

            if (report.Length < 5 ||
                report[0] != BatteryReportId ||
                report[1] != R1DeviceId ||
                report[2] != BatteryReady)
            {
                return new DeviceStatus(
                    "Attack Shark R1",
                    "Приёмник найден",
                    "Недоступно",
                    "Неизвестно",
                    "Получен отчёт, но он не прошёл проверку сигнатуры R1.");
            }

            var flags = report[3];
            var rawCharge = report[4];

            if (rawCharge > 100)
            {
                return new DeviceStatus(
                    "Attack Shark R1",
                    "Приёмник найден",
                    "Недоступно",
                    "Неизвестно",
                    $"Неизвестное значение заряда: {rawCharge}.");
            }

            var percentage = rawCharge <= 10 ? rawCharge * 10 : rawCharge;
            var charging = flags switch
            {
                0x01 => "Не заряжается",
                0x02 or 0x03 or 0x80 => "Заряжается",
                _ => "Неизвестно"
            };

            _lastKnownStatus = new DeviceStatus(
                "Attack Shark R1",
                "Мышь отвечает",
                $"{percentage}%",
                charging,
                "Данные получены из HID-отчёта; протокол требует проверки на этом экземпляре.");
            return _lastKnownStatus;
        }
        catch (TimeoutException)
        {
            if (_lastKnownStatus is not null)
            {
                return _lastKnownStatus with
                {
                    Connection = "Приёмник подключён · нет нового отчёта",
                    Detail = "Показано последнее достоверное значение: мышь могла перейти в энергосбережение."
                };
            }

            return new DeviceStatus(
                "Attack Shark R1",
                "Приёмник найден · нет ответа",
                "Недоступно",
                "Неизвестно",
                "Ожидание первого отчёта батареи от мыши.");
        }
        catch (IOException ex)
        {
            return new DeviceStatus(
                "Attack Shark R1",
                "Нет доступа к HID",
                "Недоступно",
                "Неизвестно",
                $"Windows не предоставила HID-поток: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new DeviceStatus(
                "Attack Shark R1",
                "Ошибка чтения",
                "Недоступно",
                "Неизвестно",
                $"Ошибка HID: {ex.Message}");
        }
    }
}