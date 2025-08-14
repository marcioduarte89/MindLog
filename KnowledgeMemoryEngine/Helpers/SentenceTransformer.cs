using Microsoft.Extensions.Configuration;
using MindLog.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace KnowledgeBaseEngine.Helpers
{
    public class SentenceTransformer
    {
        private HttpClient _sentenceTransformClient = new HttpClient()
        {
            BaseAddress = new Uri("http://127.0.0.1:5000/"), // fix this
        };

        public SentenceTransformer(IConfiguration configuration)
        {
            _sentenceTransformClient.BaseAddress = new Uri(configuration.GetValue<string>("embeddingsService")!);
        }

        public async Task<IEnumerable<SentenceEmbeddings>> GetEmbeddings(string sentence)
        {
            var sentenceTransform = new SentenceTransform()
            {
                Id = Guid.NewGuid(),
                Text = sentence,
            };

            var result = await _sentenceTransformClient
                .PostAsJsonAsync<IEnumerable<SentenceTransform>>("sentences-transform", [sentenceTransform],
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            return await result
                .Content
                .ReadFromJsonAsync<IEnumerable<SentenceEmbeddings>>() ??
                throw new Exception($"Could not get embedding from sentece {sentence}");
        }
    }
}
