namespace KnowledgeBaseEngine.Models
{
    public class Note
    {
        public string NoteName { get; set; } = default!;

        public DateTime NoteDate { get; set; }

        public string Text { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public string? Section { get; set; }
    }
}
