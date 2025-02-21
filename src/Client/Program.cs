using LTBlog.Client;
using LTBlog.Client.Infrastructure;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => {
    var client = new HttpClient {
        BaseAddress = new(builder.Configuration["BackendUrl"] ?? throw new InvalidOperationException("配置中没有 BackendUrl"))
    };
    // 开发时使用
    if (builder.HostEnvironment.IsDevelopment()) {
        client.DefaultRequestHeaders.Add("X-Forwarded-Host", "api.littletoxic.top");
    }

    return client;
});
builder.Services.AddFluentUIComponents();

builder.Logging.AddUILogger();

await builder.Build().RunAsync();