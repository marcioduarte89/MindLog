using Microsoft.Extensions.Configuration;
using MindLog.Shared.Models;
using WeaviateNET;

namespace KnowledgeBaseEngine.Helpers
{
    public class WeaviateClient
    {
        private readonly IConfiguration _configuration;
        private readonly WeaviateDB _db;

        public WeaviateClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _db = new WeaviateDB(_configuration.GetValue<string>("weaviateUrl")!);
        }

        public async Task<WeaviateClass<StorageModelWeaviate>> GetWeaviateClass()
        {
            await _db.Schema.Update();
            return _db.Schema.GetClass<StorageModelWeaviate>("StorageModel") ??
                     throw new Exception("Storage model does not exist");
        }

        public WeaviateDB Db => _db;
    }
}
