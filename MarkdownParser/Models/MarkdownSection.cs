namespace MarkdownParser.Models
{
    internal class MarkdownSection
    {
        public Guid Id { get; set; }

        public string Text { get; set; } = default!;

        public string NoteName { get; set; } = default!;

        public DateTime NoteDate { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public string? Section { get; set; }
    }
}
