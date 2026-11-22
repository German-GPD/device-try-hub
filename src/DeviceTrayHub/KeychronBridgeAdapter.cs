using HidSharp;

namespace German.DeviceTrayHub;

internal static class KeychronBridgeAdapter
{
    private const int VendorId = 0x3434;
    private static readonly int[] ReceiverProductIds = [0xD030, 0xD031];
    private const int ReadTimeoutMilliseconds = 1000;

    public static DeviceStatus ReadStatus()
    {
        try
        {
            var device = ReceiverProductIds
                .SelectMany(productId => DeviceList.Local.GetHidDevices(VendorId, productId))
                .FirstOrDefault(candidate =>
                    candidate.GetMaxInputReportLength() >= 33 &&
                    candidate.GetMaxOutputReportLength() >= 33);

            if (device is null)
            {
                return new DeviceStatus(
                    "Keychron K3 Max",
                    "Приёмник не обнаружен",
                    "Недоступно",
                    "Неизвестно",
                    "Keychron Link receiver (3434:D030/D031) не найден.");
            }

            using var stream = device.Open();
            stream.ReadTimeout = ReadTimeoutMilliseconds;
            stream.WriteTimeout = ReadTimeoutMilliseconds;

            // HIDSharp includes the report ID in byte 0; WebHID's reportId=0
            // and 32-byte FR payload therefore become a 33-byte HID report.
            var request = new byte[33];
            request[0] = 0x00;
            request[1] = 0xB2; // FR_GET_STATE (read-only)
            stream.Write(request);

            var response = stream.Read();
            if (!KeychronBridgeStateParser.TryParse(response, out var connected))
            {
                return new DeviceStatus(
                    "Keychron K3 Max",
                    "Приёмник найден · нет ответа",
                    "Недоступно",
                    "Неизвестно",
                    "Ответ Keychron Link не прошёл проверку формата.");
            }

            return new DeviceStatus(
                "Keychron K3 Max",
                connected ? "Клавиатура отвечает · 2.4 GHz" : "Клавиатура не подключена",
                "Недоступно",
                "Неизвестно",
                "Приёмник подтверждает состояние слота. Достоверный заряд батареи по этому каналу пока не подтверждён.");
        }
        catch (TimeoutException)
        {
            return new DeviceStatus(
                "Keychron K3 Max",
                "Приёмник найден · тайм-аут",
                "Недоступно",
                "Неизвестно",
                "Keychron Link не ответил на запрос состояния.");
        }
        catch (IOException ex)
        {
            return new DeviceStatus(
                "Keychron K3 Max",
                "Нет доступа к HID",
                "Недоступно",
                "Неизвестно",
                $"Windows не предоставила HID-поток: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new DeviceStatus(
                "Keychron K3 Max",
                "Ошибка чтения",
                "Недоступно",
                "Неизвестно",
                $"Ошибка Keychron HID: {ex.Message}");
        }
    }
}