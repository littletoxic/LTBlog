var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
    Args = args,
    // 保证开发与发布行为一致
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddLettuceEncrypt();

builder.WebHost.ConfigureKestrel(kestrel => {
    kestrel.ConfigureHttpsDefaults(options => {
        options.UseLettuceEncrypt(kestrel.ApplicationServices);
    });
});

var app = builder.Build();

app.MapReverseProxy();

app.MapStaticAssets("Client.staticwebassets.endpoints.json");
app.MapFallbackToFile("index.html");

await app.RunAsync();