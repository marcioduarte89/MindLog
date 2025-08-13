using MarkdownParser.Models;
using System.Net.Http.Json;
using System.Text.Json;
using MindLog.Shared.Models;

namespace MarkdownParser.Helpers
{
    internal static class MarkdownTransformationService
    {
        static HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("http://127.0.0.1:5000/"), // fix this
        };

        internal static async Task<IEnumerable<StorageModel>> Transform(IEnumerable<MarkdownSection> markdownSections)
        {
            var sentencesTransform = markdownSections
                .Select(x => new SentenceTransform()
                {
                    Id = x.Id,
                    Text = x.Text,
                });

            var result =
                await sharedClient
                .PostAsJsonAsync("sentences-transform", sentencesTransform, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            var sentencesEmbeddings = await result.Content.ReadFromJsonAsync<IEnumerable<SentenceEmbeddings>>();

            var storageModels = new List<StorageModel>();

            foreach (var markdownSection in markdownSections)
            {
                var sentenceEmbedding = sentencesEmbeddings
                    .FirstOrDefault(x => x.Id == markdownSection.Id) ?? 
                    throw new ArgumentException($"Could not find sentence with id {markdownSection.Id}");

                storageModels.Add(new StorageModel()
                {
                    NoteDate = markdownSection.NoteDate,
                    NoteName = markdownSection.NoteName,
                    Section = markdownSection.Section,
                    Tags = markdownSection.Tags,
                    Text = markdownSection.Text,
                    Embeddings = sentenceEmbedding.Embeddings
                });
            }

            return storageModels;
        }
    }
}
