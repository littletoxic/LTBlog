var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddLettuceEncrypt();
builder.Services.AddSystemd();

builder.WebHost.ConfigureKestrel(kestrel => {
    kestrel.ConfigureHttpsDefaults(options => {
        options.UseLettuceEncrypt(kestrel.ApplicationServices);
    });
});

var app = builder.Build();
app.UseHttpsRedirection();
app.MapReverseProxy();
await app.RunAsync();
