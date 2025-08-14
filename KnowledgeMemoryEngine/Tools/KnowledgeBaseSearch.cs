using KnowledgeBaseEngine.Helpers;
using KnowledgeBaseEngine.Models;
using MindLog.Shared.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using WeaviateNET;
using WeaviateNET.Query;
using WeaviateNET.Query.AdditionalProperty;
using WeaviateNET.Query.ConditionalOperator;

namespace KnowledgeBaseEngine.Tools
{
    [McpServerToolType]
    public class KnowledgeBaseSearch
    {
        [McpServerTool, Description("Queries the Knowledge base")]
        public static async Task<IEnumerable<Note>> Query(
            QueryTranslator queryTranslator,
            Helpers.WeaviateClient weaviateClient,
            SentenceTransformer sentenceTransformer,
            string query)
        {
            //var translatedQuery = await queryTranslator.Translate(query);
            var translatedQuery = new TranslatedQuery()
            {
                Query = "Dependency injection",
                DateRange = new DateRange()
                {
                    EndDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow.AddYears(-5)
                },
                Keywords = ["DependencyInjection"]
            };

            var sentenceEmbeddings = await sentenceTransformer.GetEmbeddings(translatedQuery.Query);

            var mc = await weaviateClient.GetWeaviateClass();

            var q = mc.CreateGetQuery();
            q.Filter.NearVector(sentenceEmbeddings.Single().Embeddings.ToArray());

            var conditions = new List<ConditionalAtom<StorageModelWeaviate>>();
            //the package doesn't yet support this
            //{
            //    When<StorageModelWeaviate, float>.GreaterThan(["_additional", "certainty"], 0.6f)
            //};

            if (translatedQuery.HasKeywords())
            {
                conditions.Add(When<StorageModelWeaviate, string[]>.ContainsAny("tags", translatedQuery.Keywords));
            }

            if(translatedQuery.HasDateRange())
            {
                conditions.AddRange(
                    When<StorageModelWeaviate, DateTime>.GreaterThanEqual("noteDate", translatedQuery.DateRange.StartDate),
                    When<StorageModelWeaviate, DateTime>.LessThanEqual("noteDate", translatedQuery.DateRange.EndDate));
            } 
            else if(translatedQuery.HasDate())
            {
                conditions.Add(When<StorageModelWeaviate, DateTime>.Equal("noteDate", translatedQuery.Date.Value));
            }

            if (!conditions.Any())
            {
                q.Filter.Limit(10);
            }
            else
            {
                q.Filter.Where(Conditional<StorageModelWeaviate>.And(conditions.ToArray()));
            }

            q.Fields.Additional.Add(Additional.Id);
            q.Fields.Additional.Add(new Additional("certainty"));

            var graphQL = new GraphQLQuery()
            {
                Query = q.ToString()
            };

            var queryResult = await weaviateClient.Db.Schema.RawQuery(graphQL);
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
