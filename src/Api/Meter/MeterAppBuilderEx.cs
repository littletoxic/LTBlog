using System.Device.Gpio;
using System.Device.I2c;
using System.Reactive.Subjects;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using LTBlog.Data;

namespace LTBlog.Api.Meter;

public static class MeterAppBuilderEx {
    public static IServiceCollection AddMeterWorker(this IServiceCollection serviceCollection) {
        serviceCollection.AddHostedService<MeterWorker>();
        serviceCollection.AddHostedService<MeterDisplayWorker>();
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
        serviceCollection.AddSingleton<ISubject<MeterData>>(_ => new Subject<MeterData>());
        serviceCollection.AddSingleton<IObservable<MeterData>>(provider =>
            provider.GetRequiredService<ISubject<MeterData>>());
        return serviceCollection;
    }
}