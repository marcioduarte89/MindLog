using MarkdownParser.Models;
using System.Net.Http.Json;
using System.Text.Json;
using MindLog.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace MarkdownParser.Helpers
{
    public class MarkdownTransformationService
    {
        private HttpClient _sharedClient = new();

        public MarkdownTransformationService(IConfiguration configuration)
        {
            _sharedClient.BaseAddress = new Uri(configuration.GetValue<string>("embeddingsService")!);
        }

        internal async Task<IEnumerable<StorageModel>> Transform(IEnumerable<MarkdownSection> markdownSections)
        {
            var sentencesTransform = markdownSections
                .Select(x => new SentenceTransform()
                {
                    Id = x.Id,
                    Text = x.Text,
                });

            var result =
                await _sharedClient
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
