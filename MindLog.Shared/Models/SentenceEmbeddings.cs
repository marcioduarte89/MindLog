namespace MindLog.Shared.Models
{
    public class SentenceEmbeddings
    {
        public Guid Id { get; init; }

        public IEnumerable<float> Embeddings { get; init; } = default!;
    }
}
