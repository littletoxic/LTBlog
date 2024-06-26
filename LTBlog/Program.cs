using LTBlog.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ArticleContext>(options =>
    options.UseNpgsql("Host=localhost;Database=articles;Username=postgres;Password=&0k42^V2EOD*AjS")
        .UseSnakeCaseNamingConvention());

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

await app.RunAsync();
