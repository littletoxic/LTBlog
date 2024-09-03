namespace LTBlog.Sensor;

using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.CharacterLcd;
using Iot.Device.Common;
using Microsoft.AspNetCore.SignalR;
using UnitsNet;

public class SensorWorker(
    ILogger<SensorWorker> logger,
    Bme280 bme280,
    Lcd2004 lcd,
    IHubContext<SensorHub> hubContext) : BackgroundService {
    private uint loop = 0;
    private int measurementDuration;

    public override async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Iot service starting");
        measurementDuration = bme280.GetMeasurementDuration();
        logger.LogInformation("Measurement duration: {MeasurementDuration}ms", measurementDuration);

        var result = await bme280.ReadAsync();
        var heatIndex = WeatherHelper.CalculateHeatIndex(
                (Temperature)result.Temperature!, (RelativeHumidity)result.Humidity!);

        lcd.SetCursorPosition(0, 0);
        lcd.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        lcd.SetCursorPosition(0, 1);
        lcd.Write($"Humidity: {result.Humidity?.Percent,5:#0.00}%");

        lcd.SetCursorPosition(0, 2);
        lcd.Write($"Temperature: {result.Temperature?.DegreesCelsius,4:0.0}\u00DFC");

        lcd.SetCursorPosition(0, 3);
        lcd.Write($"Heat Index: {heatIndex.DegreesCelsius,4:0.0}\u00DFC");

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(1000 - measurementDuration, stoppingToken);

            var result = await bme280.ReadAsync();
            var heatIndex = WeatherHelper.CalculateHeatIndex(
                (Temperature)result.Temperature!, (RelativeHumidity)result.Humidity!);
            var altValue = WeatherHelper.CalculateAltitude(
                    (Pressure)result.Pressure!, WeatherHelper.MeanSeaLevel, (Temperature)result.Temperature!);

            var state = new {
                temperature = result.Temperature?.DegreesCelsius ?? 0,
                heatIndex = heatIndex.DegreesCelsius,
                pressure = result.Pressure?.Hectopascals ?? 0,
                altitude = altValue.Meters,
                humidity = result.Humidity?.Percent ?? 0
            };

            lcd.SetCursorPosition(0, 0);
            lcd.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            lcd.SetCursorPosition(10, 1);
            lcd.Write($"{state.humidity,5:#0.00}");

            lcd.SetCursorPosition(13, 2);
            lcd.Write($"{state.temperature,4:0.0}");

            lcd.SetCursorPosition(12, 3);
            lcd.Write($"{state.heatIndex,4:0.0}");

            if (loop++ % 600 == 0) {
                logger.LogInformation("Temperature: {Temperature:0.#}\u00B0C", state.temperature);
                logger.LogInformation("Pressure: {Pressure:0.##}hPa", state.pressure);
                logger.LogInformation("Altitude: {Altitude:0.##}m", state.altitude);
                logger.LogInformation("Relative humidity: {Humidity:0.##}%", state.humidity);
                logger.LogInformation("Heat Index: {HeatIndex:0.#}\u00B0C", state.heatIndex);
            }

            await hubContext.Clients.All.SendAsync("ReceiveState", state, stoppingToken);
        }
    }
}
