using WeaviateNET;

namespace MindLog.Shared.Models
{
    public class StorageModel
    {
        public string NoteName { get; set; } = default!;

        public DateTime NoteDate { get; set; }

        public string Text { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public string? Section { get; set; }

        public IEnumerable<float> Embeddings { get; set; } = [];
    }

    [IndexNullState]
    public class StorageModelWeaviate
    {
        public string noteName;

        public DateTime noteDate;

        public string text;

        public string[] tags;

        public string? section;
    }
}
