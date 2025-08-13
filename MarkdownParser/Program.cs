using MarkdownParser.Helpers;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, Parser!");

var config = new ConfigurationBuilder()
     .AddJsonFile($"appsettings.json")
     .Build();

string? markdownPath = config.GetSection("pathToLoadMdFiles")?.Value ?? 
    throw new ArgumentNullException("lastParserLogFileName does not exist or is not configured");

var lastLogFileName = GetLastLogFileName(config);
var lastLogFileTimestamp = await FileHelpers.GetLastLogFileDateTime(lastLogFileName);

var filePathList = FileHelpers.GetAllFilesInDirectoryBasedOnLastUpdated(markdownPath, lastLogFileTimestamp);

if (!filePathList.Any())
{
    Console.WriteLine("No file changes");
    return;
}

var markdownSections = MarkdownParser.Helpers.MarkdownParser.ParseMarkdownFiles(filePathList);

var storageModels = await MarkdownTransformationService.Transform(markdownSections);

await StorageModelPersister.Save(storageModels);

File.WriteAllText(lastLogFileName, DateTime.UtcNow.ToString());

Console.ReadKey();

static string GetLastLogFileName(IConfigurationRoot config)
{
    return Path.Combine(
        config.GetSection("pathToLoadMdFiles")?.Value ?? throw new ArgumentNullException("lastParserLogFileName does not exist or is not configured"),
        config.GetSection("lastParserLogFileName")?.Value ?? throw new ArgumentNullException("lastParserLogFileName does not exist or is not configured"));
}