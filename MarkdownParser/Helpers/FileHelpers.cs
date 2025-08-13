namespace MarkdownParser.Helpers
{
    internal static class FileHelpers
    {
        internal static async Task<DateTime> GetLastLogFileDateTime(string lastLogFileName)
        {
            var lastLogFileTimestamp = DateTime.MinValue;

            if (!File.Exists(lastLogFileName))
            {
                File.WriteAllText(lastLogFileName, lastLogFileTimestamp.ToString());
            }
            else
            {
                if (!DateTime.TryParse(await File.ReadAllTextAsync(lastLogFileName), out lastLogFileTimestamp))
                {
                    throw new ArgumentException($"The file {lastLogFileName} was not in the correct format for DateTime parsing");
                }
            }

            return lastLogFileTimestamp;
        }

        internal static List<string> GetAllFilesInDirectoryBasedOnLastUpdated(string targetDirectory, DateTime lastLogFile)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.md", SearchOption.AllDirectories);

            var updatedFileList = new List<string>();

            foreach (var fileEntry in fileEntries)
            {
                var lastUpdatedTime = File.GetLastWriteTimeUtc(fileEntry);
                if (lastUpdatedTime > lastLogFile)
                {
                    updatedFileList.Add(fileEntry);
                }
            }

            return updatedFileList;
        }
    }
}
