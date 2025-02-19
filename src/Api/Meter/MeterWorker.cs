using System.Reactive.Subjects;
using Iot.Device.Bmxx80;
using Iot.Device.Common;
using LTBlog.Data;

namespace LTBlog.Api.Meter;

public class MeterWorker(
    ILogger<MeterWorker> logger,
    Bme280 bme280,
    ISubject<MeterData> subject) : BackgroundService {
    private int _measurementDuration;
    private uint _loop;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        logger.LogInformation("Meter service starting");
        _measurementDuration = bme280.GetMeasurementDuration();
        logger.LogInformation("Measurement duration: {MeasurementDuration}ms", _measurementDuration);

        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(1000 - _measurementDuration, stoppingToken);

            try {
                if (!bme280.TryReadTemperature(out var temperature) ||
                    !bme280.TryReadPressure(out var pressure) ||
                    !bme280.TryReadHumidity(out var humidity)) {
                    throw new InvalidOperationException("Failed to read sensor data");
                }

                var heatIndex = WeatherHelper.CalculateHeatIndex(temperature, humidity);
                var altValue = WeatherHelper.CalculateAltitude(pressure, WeatherHelper.MeanSeaLevel, temperature);

                var data = new MeterData(
                    altValue.Meters,
                    heatIndex.DegreesCelsius,
                    humidity.Percent,
                    pressure.Hectopascals,
                    temperature.DegreesCelsius);

                if (_loop++ % 600 == 0) {
                    logger.LogInformation("Temperature: {Temperature:0.#}\u00B0C", data.Temperature);
                    logger.LogInformation("Pressure: {Pressure:0.##}hPa", data.Pressure);
                    logger.LogInformation("Altitude: {Altitude:0.##}m", data.Altitude);
                    logger.LogInformation("Relative humidity: {Humidity:0.##}%", data.Humidity);
                    logger.LogInformation("Heat Index: {HeatIndex:0.#}\u00B0C", data.HeatIndex);
                }

                subject.OnNext(data);
            } catch (Exception ex) {
                logger.LogError(ex, "Error occurred while reading sensor data");
                throw;
            }
        }
    }
}