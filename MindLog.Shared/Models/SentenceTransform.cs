namespace MindLog.Shared.Models
{
    public record SentenceTransform
    {
        public Guid Id { get; init; }

        public string Text { get; init; } = default!;
    }
}
