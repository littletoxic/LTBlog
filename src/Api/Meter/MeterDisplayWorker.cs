using LTBlog.Data;
using Iot.Device.CharacterLcd;
using Microsoft.AspNetCore.SignalR;

namespace LTBlog.Api.Meter;

public class MeterDisplayWorker(
    ILogger<MeterDisplayWorker> logger,
    IHubContext<MeterHub, IDataClient> hubContext,
    Lcd2004 lcd,
    IObservable<MeterData> observable) : BackgroundService {
    private IDisposable? _lcdSubscription;
    private IDisposable? _hubSubscription;

    public override async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("service starting");

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
        _lcdSubscription = observable.Subscribe(state => {
            lcd.SetCursorPosition(0, 0);
            lcd.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            lcd.SetCursorPosition(10, 1);
            lcd.Write($"{state.Humidity,5:#0.00}");

            lcd.SetCursorPosition(13, 2);
            lcd.Write($"{state.Temperature,4:0.0}");

            lcd.SetCursorPosition(12, 3);
            lcd.Write($"{state.HeatIndex,4:0.0}");
        });
        _hubSubscription = observable.Subscribe(data =>
            hubContext.Clients.All.ReceiveData(data).ConfigureAwait(false).GetAwaiter().GetResult());

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken) {
        _lcdSubscription?.Dispose();
        _hubSubscription?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}