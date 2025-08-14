using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownParser.Helpers
{
    internal class Parser
    {
        private readonly MarkdownTransformationService _markdownService;
        private readonly IConfiguration _configuration;

        public Parser(
            MarkdownTransformationService markdownService,
            IConfiguration configuration)
        {
            _markdownService = markdownService;
            _configuration = configuration;
        }

        public async Task Start()
        {
            string? markdownPath = _configuration.GetSection("pathToLoadMdFiles")?.Value ??
            throw new ArgumentNullException("lastParserLogFileName does not exist or is not configured");

            var lastLogFileName = GetLastLogFileName();
            var lastLogFileTimestamp = await FileHelpers.GetLastLogFileDateTime(lastLogFileName);

            var filePathList = FileHelpers.GetAllFilesInDirectoryBasedOnLastUpdated(markdownPath, lastLogFileTimestamp);

            if (!filePathList.Any())
            {
                Console.WriteLine("No file changes");
                return;
            }

            var markdownSections = MarkdownParser.ParseMarkdownFiles(filePathList);

            var storageModels = await _markdownService.Transform(markdownSections);

            await StorageModelPersister.Save(storageModels);

            File.WriteAllText(lastLogFileName, DateTime.UtcNow.ToString());
        }

        private string GetLastLogFileName()
        {
            return Path.Combine(
                _configuration.GetSection("pathToLoadMdFiles")?.Value ?? throw new ArgumentNullException("lastParserLogFileName does not exist or is not configured"),
                _configuration.GetSection("lastParserLogFileName")?.Value ?? throw new ArgumentNullException("lastParserLogFileName does not exist or is not configured"));
        }
    }
}
