using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownParser.Models;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarkdownParser.Helpers
{
    internal static class MarkdownParser
    {
        internal static IEnumerable<MarkdownSection> ParseMarkdownFiles(List<string> filePathList)
        {
            var markdownSections = new List<MarkdownSection>();

            foreach (var filePath in filePathList)
            {
                markdownSections.AddRange(ParseMarkdownFiles(filePath));
            }

            return markdownSections;
        }

        static IEnumerable<MarkdownSection> ParseMarkdownFiles(string filePath)
        {
            var markdown = File.ReadAllText(filePath);

            var pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();

            var writer = new StringWriter();
            var renderer = new HtmlRenderer(writer);
            pipeline.Setup(renderer);

            var document = Markdown.Parse(markdown, pipeline);

            var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var metadata = deserializer.Deserialize<FrontMatterBlock>(yamlBlock.Lines.ToString());

            var correctedDate = DateTime.SpecifyKind(metadata.Date, DateTimeKind.Utc);
            metadata = metadata with { Date = correctedDate };

            string currentHeading = null;
            var currentTextLines = new List<string>();

            var result = new List<MarkdownSection>();

            void AddSection()
            {
                if (currentHeading != null && currentTextLines.Any())
                {
                    result.Add(new MarkdownSection
                    {
                        Id = Guid.NewGuid(),
                        Section = currentHeading,
                        Text = string
                            .Join("\n\n", currentTextLines)
                            .Replace("\r", "")
                            .Replace("\n", " ")
                            .Replace("  ", " ") // collapse multiple spaces
                            .Trim(),
                        NoteName = Path.GetFileName(filePath),
                        NoteDate = metadata.Date,
                        Tags = metadata.Tags
                    });
                    currentTextLines.Clear();
                }
            }

            foreach (var block in document)
            {
                switch (block)
                {
                    case HeadingBlock heading:
                        AddSection(); // save previous
                        if (heading.Inline != null)
                        {
                            currentHeading = CleanInlineText(heading.Inline);
                        }
                        break;

                    case ParagraphBlock paragraph:
                        var paraText = CleanInlineText(paragraph.Inline);
                        currentTextLines.Add(paraText);
                        break;

                    case ListBlock list:
                        foreach (var item in list)
                        {
                            if (item is ListItemBlock listItem)
                            {
                                var itemText = string.Join(" ", listItem.Select(x => x is ParagraphBlock pb
                                    ? string.Concat(pb.Inline.Select(i => i.ToString()))
                                    : x.ToString()));
                                currentTextLines.Add($"- {itemText}");
                            }
                        }
                        break;

                    case FencedCodeBlock code:
                        var codeLang = code.Info ?? "code";
                        var codeText = string.Join('\n', code.Lines.Lines.Select(l => l.ToString().TrimEnd()));
                        currentTextLines.Add($"[{codeLang} code]\n{codeText}");
                        break;

                }
            }

            // Add the last section
            AddSection();

            return result;
        }

        static string CleanInlineText(ContainerInline container)
        {
            var builder = new StringBuilder();

            foreach (var inline in container)
            {
                switch (inline)
                {
                    case LiteralInline literal:
                        builder.Append(literal.Content.Text.Substring(literal.Content.Start, literal.Content.Length));
                        break;

                    case EmphasisInline emphasis:
                        builder.Append(CleanInlineText(emphasis)); // strip formatting
                        break;

                    case LinkInline link:
                        if (link.IsImage)
                        {
                            // Format as: [Image: alt text] (url)
                            var altText = CleanInlineText(link);
                            builder.Append($"[Image: {altText}] ({link.Url})");
                        }
                        else
                        {
                            var linkText = CleanInlineText(link);
                            if (!string.IsNullOrWhiteSpace(link.Url))
                                builder.Append($"{linkText} ({link.Url})");
                            else
                                builder.Append(linkText);
                        }
                        break;

                    case CodeInline code:
                        builder.Append(code.Content);
                        break;

                    case LineBreakInline:
                        builder.Append(' ');
                        break;

                    default:
                        builder.Append(inline.ToString());
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
