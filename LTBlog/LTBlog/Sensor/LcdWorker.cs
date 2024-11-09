namespace LTBlog.Sensor;

using System.Threading;
using System.Threading.Channels;
using Iot.Device.CharacterLcd;
using Client.Model;

public class LcdWorker(
    ILogger<LcdWorker> logger,
    Lcd2004 lcd,
    Channel<SensorState> channel) : BackgroundService {

    public override async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Lcd service starting");
  
        lcd.SetCursorPosition(0, 0);
        lcd.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        lcd.SetCursorPosition(0, 1);
        lcd.Write($"Humidity: {0,5:#0.00}%");

        lcd.SetCursorPosition(0, 2);
        lcd.Write($"Temperature: {0,4:0.0}\u00DFC");

        lcd.SetCursorPosition(0, 3);
        lcd.Write($"Heat Index: {0,4:0.0}\u00DFC");

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            var state = await channel.Reader.ReadAsync(stoppingToken);

            lcd.SetCursorPosition(0, 0);
            lcd.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            lcd.SetCursorPosition(10, 1);
            lcd.Write($"{state.Humidity,5:#0.00}");

            lcd.SetCursorPosition(13, 2);
            lcd.Write($"{state.Temperature,4:0.0}");

            lcd.SetCursorPosition(12, 3);
            lcd.Write($"{state.HeatIndex,4:0.0}");
        }
    }
}
