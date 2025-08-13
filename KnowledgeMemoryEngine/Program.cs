using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5001");
builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddMcpServer()
    .WithHttpTransport(options => { options.Stateless = true; })
    .WithToolsFromAssembly();

var app = builder.Build();
app.MapControllers();
app.MapMcp("/mcp");
app.Run();

//var builder = Host.CreateEmptyApplicationBuilder(settings: null);

//builder
//    .Services
//    .AddMcpServer()
//    .WithStdioServerTransport()
//    .WithToolsFromAssembly();

//var config = new ConfigurationBuilder()
//     .AddJsonFile($"appsettings.json")
//     .Build();

//builder.Services.AddSingleton(_ =>
//{
//    var client = new HttpClient() { BaseAddress = new Uri(config.GetSection("embeddingsService").Value) };
//    return client;
//});

//var app = builder.Build();

//await app.RunAsync();