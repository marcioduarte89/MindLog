namespace MarkdownParser.Models
{
    internal record FrontMatterBlock {

        public DateTime Date { get; init; }
        public string Resource { get; init; }
        public IEnumerable<string> Tags { get; init; }

    }
}
