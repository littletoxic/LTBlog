using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddLettuceEncrypt();
builder.Services.AddSystemd();

builder.WebHost.ConfigureKestrel(kestrel => {
    kestrel.ConfigureHttpsDefaults(options => {
        options.UseLettuceEncrypt(kestrel.ApplicationServices);
    });
    kestrel.ListenAnyIP(80);
    kestrel.ListenAnyIP(443, options => {
        options.Protocols = HttpProtocols.Http1AndHttp2;
        options.UseHttps();
    });
});

var app = builder.Build();
app.UseHttpsRedirection();
app.MapReverseProxy();
await app.RunAsync();
