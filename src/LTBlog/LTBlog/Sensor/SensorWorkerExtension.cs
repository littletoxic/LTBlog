﻿namespace LTBlog.Sensor;

using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Bmxx80;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading.Channels;
using Client.Model;

public static class SensorWorkerExtension {

    public static IServiceCollection AddSensorWorker(this IServiceCollection serviceCollection) {
        serviceCollection.AddHostedService<SensorWorker>();
        serviceCollection.AddHostedService<LcdWorker>();
        serviceCollection.AddSingleton(_ => {
            var iic = I2cDevice.Create(new(0, Bmx280Base.SecondaryI2cAddress));
            var bme280 = new Bme280(iic) {
                TemperatureSampling = Sampling.HighResolution,
                PressureSampling = Sampling.UltraHighResolution,
                HumiditySampling = Sampling.HighResolution,
                StandbyTime = StandbyTime.Ms500
            };
            bme280.SetPowerMode(Bmx280PowerMode.Normal);
            return bme280;
        });
        serviceCollection.AddSingleton(_ => {
            var iic = I2cDevice.Create(new(1, 0x27));
            var driver = new Pcf8574(iic);
            return new Lcd2004(
                0,
                2,
                [4, 5, 6, 7],
                3,
                0.1f,
                1,
                new(PinNumberingScheme.Logical, driver));
        });
        serviceCollection.AddSingleton(_ => Channel.CreateUnbounded<SensorState>());
        return serviceCollection;
    }
}
