using HidSharp;
using System.IO;

namespace German.DeviceTrayHub;

internal static class R1BatteryAdapter
{
    private const int VendorId = 0x1D57;
    private const int WirelessProductId = 0xFA60;
    private const string InterfaceMarker = "&mi_02";
    private const string BatteryCollectionMarker = "&col03";
    private const int ReadTimeoutMilliseconds = 3000;

    private static readonly object StateLock = new();
    private static readonly ManualResetEventSlim FirstReading = new(false);
    private static Thread? _listener;
    private static DeviceStatus? _lastKnownStatus;
    private static (int Percentage, byte State)? _candidate;
    private static int _candidateCount;

    public static DeviceStatus ReadStatus()
    {
        EnsureListenerStarted();
        _ = FirstReading.Wait(TimeSpan.FromMilliseconds(2800));

        lock (StateLock)
        {
            return _lastKnownStatus ?? new DeviceStatus(
                "Attack Shark R1",
                "Приёмник найден · жду пакет батареи",
                "Недоступно",
                "Неизвестно",
                "Слушаю пассивный HID-канал батареи (MI_02 / COL03). Значение появится после отчёта мыши.");
        }
    }

    private static void EnsureListenerStarted()
    {
        lock (StateLock)
        {
            if (_listener is { IsAlive: true }) return;

            _listener = new Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "Attack Shark R1 battery listener"
            };
            _listener.Start();
        }
    }

    private static void ListenLoop()
    {
        while (true)
        {
            try
            {
                var device = DeviceList.Local
                    .GetHidDevices(VendorId, WirelessProductId)
                    .FirstOrDefault(candidate =>
                        candidate.GetMaxInputReportLength() >= 5 &&
                        candidate.DevicePath.Contains(InterfaceMarker, StringComparison.OrdinalIgnoreCase) &&
                        candidate.DevicePath.Contains(BatteryCollectionMarker, StringComparison.OrdinalIgnoreCase));

                if (device is null)
                {
                    SetUnavailableIfNoReceiver();
                    Thread.Sleep(1500);
                    continue;
                }

                using var stream = device.Open();
                stream.ReadTimeout = ReadTimeoutMilliseconds;

                while (true)
                {
                    var report = stream.Read();
                    if (report.Length < 5) continue;

                    if (!R1BatteryReportParser.TryParse(report, out var reading))
                        continue;

                    AcceptReading(reading);
                }
            }
            catch (TimeoutException)
            {
                // Read is blocking and passively receives the mouse's own periodic reports.
                // A timeout is not a battery value and must never be converted to 0%.
            }
            catch (IOException)
            {
                // Receiver may have been unplugged or claimed by another application.
                Thread.Sleep(1200);
            }
            catch
            {
                // Keep the tray app alive; retry device discovery without fabricating data.
                Thread.Sleep(1500);
            }
        }
    }

    private static void AcceptReading(R1BatteryReading reading)
    {
        lock (StateLock)
        {
            var pair = (reading.Percentage, reading.RawState);
            if (_candidate == pair)
                _candidateCount++;
            else
            {
                _candidate = pair;
                _candidateCount = 1;
            }

            // On reconnect the receiver can emit a single bogus 100% frame.
            // Accept only a repeated identical state/level pair.
            if (_candidateCount < 2) return;

            var charging = reading.RawState switch
            {
                0x01 => "Не заряжается",
                0x02 => "Питание подключено · не заряжается",
                0x03 or 0x80 => "Заряжается",
                _ => "Неизвестно"
            };

            _lastKnownStatus = new DeviceStatus(
                "Attack Shark R1",
                "Подключён · 2.4 GHz",
                $"{reading.Percentage}%",
                charging,
                "Заряд из повторно подтверждённых входящих HID-пакетов батареи (MI_02 / COL03).");

            FirstReading.Set();
        }
    }

    private static void SetUnavailableIfNoReceiver()
    {
        lock (StateLock)
        {
            _candidate = null;
            _candidateCount = 0;
            _lastKnownStatus = new DeviceStatus(
                "Attack Shark R1",
                "Приёмник не обнаружен",
                "Недоступно",
                "Неизвестно",
                "USB-приёмник 1D57:FA60 не найден. Не подставляю сохранённый процент как текущий.");
            FirstReading.Set();
        }
    }
}