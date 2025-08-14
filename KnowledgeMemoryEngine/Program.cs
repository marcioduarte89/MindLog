using KnowledgeBaseEngine.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5001");
builder.Services.AddOptions();
builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434/"), "llama3.2:3b"));
builder.Services.AddSingleton<QueryTranslator>();
builder.Services.AddScoped<WeaviateClient>();
builder.Services.AddSingleton<SentenceTransformer>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddMcpServer()
    .WithHttpTransport(options => { options.Stateless = true; })
    .WithToolsFromAssembly();

var app = builder.Build();
app.MapControllers();
app.MapMcp("/mcp");
app.Run();

//var builder = Host.CreateApplicationBuilder(args);

//builder.Logging.AddConsole(consoleLogOptions =>
//{
//    // Configure all logs to go to stderr
//    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
//});

//builder
//    .Services
//    .AddMcpServer()
//    .WithStdioServerTransport()
//    .WithToolsFromAssembly();

//var config = new ConfigurationBuilder()
//     .AddJsonFile($"appsettings.json")
//     .Build();

//builder.Services.AddOptions();
//builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434/"), "llama3.2:3b"));
//builder.Services.AddSingleton<QueryTranslator>();
//builder.Services.AddScoped<WeaviateClient>();
//builder.Services.AddSingleton<SentenceTransformer>();

//var app = builder.Build();

//await app.RunAsync();