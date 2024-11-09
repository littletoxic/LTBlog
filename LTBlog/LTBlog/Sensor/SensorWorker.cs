namespace LTBlog.Sensor;

using System.Threading;
using System.Threading.Channels;
using Iot.Device.Bmxx80;
using Iot.Device.Common;
using Client.Model;
using Microsoft.AspNetCore.SignalR;
using UnitsNet;

public class SensorWorker(
    ILogger<SensorWorker> logger,
    Bme280 bme280,
    IHubContext<SensorHub, IStateClient> hubContext,
    Channel<SensorState> channel) : BackgroundService {
    private uint _loop;
    private int _measurementDuration;

    public override async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Sensor service starting");

        _measurementDuration = bme280.GetMeasurementDuration();

        logger.LogInformation("Measurement duration: {MeasurementDuration}ms", _measurementDuration);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(1000 - _measurementDuration, stoppingToken);

            var result = await bme280.ReadAsync();
            var heatIndex = WeatherHelper.CalculateHeatIndex(
                (Temperature)result.Temperature!, (RelativeHumidity)result.Humidity!);
            var altValue = WeatherHelper.CalculateAltitude(
                    (Pressure)result.Pressure!, WeatherHelper.MeanSeaLevel, (Temperature)result.Temperature!);

            var state = new SensorState {
                Temperature = result.Temperature?.DegreesCelsius ?? 0,
                HeatIndex = heatIndex.DegreesCelsius,
                Pressure = result.Pressure?.Hectopascals ?? 0,
                Altitude = altValue.Meters,
                Humidity = result.Humidity?.Percent ?? 0
            };

            if (_loop++ % 600 == 0) {
                logger.LogInformation("Temperature: {Temperature:0.#}\u00B0C", state.Temperature);
                logger.LogInformation("Pressure: {Pressure:0.##}hPa", state.Pressure);
                logger.LogInformation("Altitude: {Altitude:0.##}m", state.Altitude);
                logger.LogInformation("Relative humidity: {Humidity:0.##}%", state.Humidity);
                logger.LogInformation("Heat Index: {HeatIndex:0.#}\u00B0C", state.HeatIndex);
            }

            await channel.Writer.WriteAsync(state, stoppingToken);
            await hubContext.Clients.All.ReceiveState(state);
        }
    }
}
