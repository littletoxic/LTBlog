using LTBlog.Api.Meter;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// MeterWorker 中可能抛出异常，这里不希望主机停止
builder.Host.ConfigureHostOptions(host => {
    host.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMeterWorker(builder.Environment);

builder.Services.AddSignalR()
    .AddMessagePackProtocol();

builder.Services.AddResponseCompression();

builder.Services.AddOpenTelemetry()
    .UseOtlpExporter()
    .ConfigureResource(resource => resource
        .AddService(builder.Environment.ApplicationName)
        .AddProcessRuntimeDetector()
        .AddContainerDetector()
        .AddHostDetector())
    .WithLogging()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation())
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
} else {
    app.UseResponseCompression();
}

app.UseAuthorization();

app.MapControllers();
app.MapHub<MeterHub>("/meter-hub");

await app.RunAsync();