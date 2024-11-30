using LTBlog.Components;
using LTBlog.Data;
using LTBlog.Sensor;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureHostOptions(host => {
    host.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddDbContext<ArticleContext>(options =>
    options.UseNpgsql("Host=localhost;Database=articles;Username=postgres;Password=&0k42^V2EOD*AjS")
        .UseSnakeCaseNamingConvention());

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR()
    .AddMessagePackProtocol();

builder.Services.AddResponseCompression();

builder.Services.AddSensorWorker();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new("/app/secrets/DataProtection-Keys"));

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
    app.UseWebAssemblyDebugging();
} else {
    app.UseResponseCompression();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(LTBlog.Client._Imports).Assembly);

app.MapHub<SensorHub>("/sensorhub");

await app.RunAsync();