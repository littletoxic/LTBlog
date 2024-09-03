﻿namespace LTBlog.Sensor;

using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.CharacterLcd;
using Iot.Device.Common;
using Microsoft.Extensions.Hosting.Systemd;
using UnitsNet;

public class SensorWorker(
    ILogger<SensorWorker> logger,
    Bme280 bme280,
    Lcd2004 lcd) : BackgroundService {
    private uint loop = 0;
    private int measurementDuration;

    public override async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Iot service starting");
        measurementDuration = bme280.GetMeasurementDuration();
        logger.LogInformation("Measurement duration: {MeasurementDuration}ms", measurementDuration);
        logger.LogInformation("Systemd support enabled: {Enabled}", SystemdHelpers.IsSystemdService());

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

            lcd.SetCursorPosition(0, 0);
            lcd.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            lcd.SetCursorPosition(10, 1);
            lcd.Write($"{result.Humidity?.Percent,5:#0.00}");

            lcd.SetCursorPosition(13, 2);
            lcd.Write($"{result.Temperature?.DegreesCelsius,4:0.0}");

            lcd.SetCursorPosition(12, 3);
            lcd.Write($"{heatIndex.DegreesCelsius,4:0.0}");

            if (loop++ % 600 == 0) {
                var altValue = WeatherHelper.CalculateAltitude(
                    (Pressure)result.Pressure!, WeatherHelper.MeanSeaLevel, (Temperature)result.Temperature!);
                logger.LogInformation("Temperature: {Temperature:0.#}\u00B0C", result.Temperature?.DegreesCelsius);
                logger.LogInformation("Pressure: {Pressure:0.##}hPa", result.Pressure?.Hectopascals);
                logger.LogInformation("Altitude: {Altitude:0.##}m", altValue.Meters);
                logger.LogInformation("Relative humidity: {Humidity:0.##}%", result.Humidity?.Percent);
                logger.LogInformation("Heat Index: {HeatIndex:0.#}\u00B0C", heatIndex.DegreesCelsius);
            }
        }
    }
}
