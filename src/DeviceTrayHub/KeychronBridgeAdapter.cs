using HidSharp;
using System.IO;

namespace German.DeviceTrayHub;

internal static class KeychronBridgeAdapter
{
    private const int VendorId = 0x3434;
    private static readonly int[] ReceiverProductIds = [0xD030, 0xD031];
    private const int ReadTimeoutMilliseconds = 300;
    private const int MaxUnrelatedReports = 4;

    public static DeviceStatus ReadStatus()
    {
        try
        {
            var device = ReceiverProductIds
                .SelectMany(productId => DeviceList.Local.GetHidDevices(VendorId, productId))
                .FirstOrDefault(candidate =>
                    candidate.GetMaxInputReportLength() >= 3 &&
                    candidate.GetMaxOutputReportLength() >= 33 &&
                    candidate.GetMaxFeatureReportLength() >= 21);

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

            // Keychron Link receiver setup: feature report 0x51 / 0x01,
            // followed by output report B2 01. The receiver answers with
            // input report 0x54 (channel and link/carrier state).
            var feature = new byte[21];
            feature[0] = 0x51;
            feature[1] = 0x01;
            stream.SetFeature(feature);

            var request = new byte[33];
            request[0] = 0x00; // HID report ID 0
            request[1] = 0xB2;
            request[2] = 0x01;
            stream.Write(request);

            for (var attempt = 0; attempt <= MaxUnrelatedReports; attempt++)
            {
                byte[] report;
                try
                {
                    report = stream.Read();
                }
                catch (TimeoutException)
                {
                    return ReceiverNoReply("Keychron Link не ответил отчётом 0x54.");
                }

                if (!KeychronBridgeStateParser.TryParse(report, out var connected))
                    continue;

                return new DeviceStatus(
                    "Keychron K3 Max",
                    connected ? "Связь подтверждена · 2.4 GHz" : "Клавиатура не отвечает",
                    "Недоступно",
                    "Неизвестно",
                    "Получен ответ Link receiver 0x54. Процент батареи из этого отчёта не извлекается.");
            }

            return ReceiverNoReply("Получены только посторонние HID-отчёты, отчёт 0x54 не найден.");
        }
        catch (TimeoutException)
        {
            return ReceiverNoReply("Keychron Link не ответил на запрос состояния.");
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

    private static DeviceStatus ReceiverNoReply(string detail) =>
        new(
            "Keychron K3 Max",
            "Приёмник найден · нет ответа",
            "Недоступно",
            "Неизвестно",
            detail);
}
