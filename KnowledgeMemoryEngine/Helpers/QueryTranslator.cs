using KnowledgeBaseEngine.Models;
using Microsoft.Extensions.AI;

namespace KnowledgeBaseEngine.Helpers
{
    public class QueryTranslator
    {
        private readonly string Prompt = $$"""
            You will receive a sentence, you will attempt to extract specific keywords and date ranges out of that sentence.
            Inference should also be added to keywords, example:
            Ex: If query has c#, the keyword Dotnet should also be provided.
            If the query is about Dependency injection but has c# code, CSharp and Dotnet keywords should also be provided.
            You should also cleanse the keywords and date ranges from the query.
            The output should be given in a specific json format.
            Supported keywords: Dotnet, Async, CSharp, LanguageFeatures, DependencyInjection, ASPNETCore, EFCore, Migrations 
            Some examples:
            In: Give me all the c# sharp notes written in the last 60 days.
            Out:
            {
                "query": "c#",
                "dataRange": {
                  "startDate": DateTime.UTC.Now - 60d,
                  "endDate": DateTime.UTC,
                }
                "keywords": ["CSharp", "Dotnet"]
            }
            In: How does services.AddTransient<IMyService, MyService>() work?
            Out:
            {
                "query": "How does services.AddTransient<IMyService, MyService>() work"
                "keywords": ["csharp", "DependencyInjection", "Dotnet"]
            }
            In: What notes did I write on the 1st of December 2024?
            Out:
            {
                "query": ""
                "keywords": ["csharp", "DependencyInjection", "Dotnet"]
                "date": DateTime.UTC,
            }
            
            Query to translate: 
            """;

        private readonly IChatClient _clientClient;

        public QueryTranslator(IChatClient clientClient)
        {
            _clientClient = clientClient;
        }

        public async Task<TranslatedQuery> Translate(string query)
        {
            var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, "You are a helpful assistant."),
                    new(ChatRole.User, $"{Prompt} {query}")
                };

            var result = await _clientClient.GetResponseAsync<TranslatedQuery>(messages);
            return result.Result;
        }
    }
}
