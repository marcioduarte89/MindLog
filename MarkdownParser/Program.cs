using MarkdownParser.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, Parser!");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<MarkdownTransformationService>();
builder.Services.AddSingleton<Parser>();

var host = builder.Build();

var parser = host.Services.GetRequiredService<Parser>();
await parser.Start();

await host.RunAsync();