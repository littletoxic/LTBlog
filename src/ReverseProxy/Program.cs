var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
    Args = args,
    // 保证开发与发布行为一致
    ContentRootPath = AppContext.BaseDirectory
});

//StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddLettuceEncrypt();

builder.WebHost.ConfigureKestrel(kestrel => {
    kestrel.ConfigureHttpsDefaults(options => { options.UseLettuceEncrypt(kestrel.ApplicationServices); });
});

builder.Services.AddCors(options => {
    options.AddPolicy("backend", policy => policy.WithOrigins("api.littletoxic.top"));
});

var app = builder.Build();

app.UseCors();

app.MapReverseProxy();
app.MapStaticAssets("Client.staticwebassets.endpoints.json")
    .WithMetadata(new HostAttribute("littletoxic.top", "localhost"));
app.MapFallbackToFile("index.html").WithMetadata(new HostAttribute("littletoxic.top", "localhost"));

await app.RunAsync();