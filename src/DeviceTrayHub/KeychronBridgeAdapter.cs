using HidSharp;
using System.IO;

namespace German.DeviceTrayHub;

internal static class KeychronBridgeAdapter
{
    private const int VendorId = 0x3434;
    private static readonly int[] ReceiverProductIds = [0xD030, 0xD031];
    private const int ReadTimeoutMilliseconds = 300;
    private const int MaxUnrelatedReports = 3;

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

            // The documented flow negotiates receiver protocol before requesting slots.
            if (SendReadCommand(stream, 0xB1) is null)
            {
                return ReceiverNoReply("Приёмник не ответил на проверку версии протокола.");
            }

            var response = SendReadCommand(stream, 0xB2);
            if (response is null ||
                !KeychronBridgeStateParser.TryParse(response, out var connected))
            {
                return ReceiverNoReply("Ответ Keychron Link не прошёл проверку формата.");
            }

            return new DeviceStatus(
                "Keychron K3 Max",
                connected ? "Клавиатура отвечает · 2.4 GHz" : "Клавиатура не подключена",
                "Недоступно",
                "Неизвестно",
                "Приёмник подтвердил состояние слота. Достоверный заряд батареи по этому каналу пока не подтверждён.");
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

    private static byte[]? SendReadCommand(HidStream stream, byte command)
    {
        var request = new byte[33];
        request[0] = 0x00; // HID report ID
        request[1] = command;
        stream.Write(request);

        for (var attempt = 0; attempt <= MaxUnrelatedReports; attempt++)
        {
            try
            {
                var report = stream.Read();
                if (report.Length >= 2 && report[0] == 0x00 && report[1] == command)
                    return report;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        return null;
    }

    private static DeviceStatus ReceiverNoReply(string detail) =>
        new(
            "Keychron K3 Max",
            "Приёмник найден · нет ответа",
            "Недоступно",
            "Неизвестно",
            detail);
}