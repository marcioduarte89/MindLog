using KnowledgeBaseEngine.Helpers;
using KnowledgeBaseEngine.Models;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using MindLog.Shared.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using WeaviateNET;
using WeaviateNET.Query.ConditionalOperator;

namespace KnowledgeBaseEngine.Tools
{
    [McpServerToolType]
    public class KnowledgeBaseSearch
    {
        static HttpClient sentenceTransformClient = new()
        {
            BaseAddress = new Uri("http://127.0.0.1:5000/"), // fix this
        };

        [McpServerTool, Description("Queries the Knowledge base")]
        public static async Task<IEnumerable<Note>> Query(string query)
        {
            var sentence = new SentenceTransform()
            {
                Id = Guid.NewGuid(),

                Text = query,
            };

            var result = await sentenceTransformClient
                .PostAsJsonAsync<IEnumerable<SentenceTransform>>("sentences-transform", [sentence], 
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            var sentencesEmbeddings = await result
                .Content
                .ReadFromJsonAsync<IEnumerable<SentenceEmbeddings>>() ?? 
                throw new Exception($"Could not get embedding from sentece {sentence}");

            var weaviateDB = new WeaviateDB("http://localhost:8080/v1");
            await weaviateDB.Schema.Update();
            var mc = weaviateDB.Schema.GetClass<StorageModelWeaviate>("StorageModel") ??
                     throw new Exception("Storage model does not exist");

            var dateTimeFilter = DateTimeConditionBuilder.GetDateTimeFilter(query);

            var q = mc.CreateGetQuery();
            q.Filter.NearVector(sentencesEmbeddings.Single().Embeddings.ToArray());

            if(dateTimeFilter != null)
            {
                q.Filter.Where(dateTimeFilter);
            }
            else
            {
                // If there are no dates to filter, limit by just 10 docs
                q.Filter.Limit(10);
            }

            var graphQL = new GraphQLQuery()
            {
                Query = q.ToString()
            };

            var queryResult = await weaviateDB.Schema.RawQuery(graphQL);
            var queryResultGet = queryResult.Data["Get"];
            var queryResultGetData = queryResultGet["StorageModel"];
            var weaviateModel = queryResultGetData.ToObject<StorageModelWeaviate[]>() ??
                throw new Exception("Weaviate model could not be deserialized");

            return weaviateModel.Select(x => new Note()
            {
                NoteDate = x.noteDate,
                NoteName = x.noteName,
                Section = x.section,
                Tags = x.tags,
                Text = x.text,
            });
        }
    }
}
