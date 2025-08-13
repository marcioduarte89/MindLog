using WeaviateNET;

namespace KnowledgeBaseEngine.Models
{
    [IndexNullState]
    public class WeaviateSentenceEmbeddings
    {
        public Guid id { get; init; }

        public IEnumerable<double> Embeddings { get; init; } = default!;
    }
}
