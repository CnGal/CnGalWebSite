using HtmlAgilityPack;
using System.Text;

namespace CnGalWebSite.MainSite.Shared.Components.Features.Examine;

/// <summary>Read the legacy diff envelope without rendering the enclosed source as HTML.</summary>
public static class RecordRawDiff
{
    public static IReadOnlyList<RecordTextSegment> Parse(string? content)
    {
        if (string.IsNullOrEmpty(content)) return [];
        var document = new HtmlDocument();
        document.LoadHtml(content);
        var segments = new List<RecordTextSegment>();
        var buffer = new StringBuilder();
        var bufferedChange = RecordTextChangeKind.Unchanged;
        Read(document.DocumentNode, RecordTextChangeKind.Unchanged);
        Flush();
        return segments;

        void Flush()
        {
            if (buffer.Length == 0) return;
            segments.Add(new(buffer.ToString(), bufferedChange));
            buffer.Clear();
        }

        void Append(string text, RecordTextChangeKind change)
        {
            if (text.Length == 0) return;
            if (change != bufferedChange) Flush();
            bufferedChange = change;
            buffer.Append(text);
        }

        void Read(HtmlNode node, RecordTextChangeKind change)
        {
            if (node is HtmlTextNode text)
            {
                Append(HtmlEntity.DeEntitize(text.Text), change);
                return;
            }
            if (node.NodeType == HtmlNodeType.Document)
            {
                foreach (var child in node.ChildNodes) Read(child, change);
                return;
            }
            if (node.Name is "ins" or "del")
            {
                foreach (var child in node.ChildNodes)
                    Read(child, node.Name == "ins" ? RecordTextChangeKind.Added : RecordTextChangeKind.Removed);
                return;
            }
            // The API substitutes line breaks with br and wraps formatted JSON
            // in pre/code. These are transport formatting, not part of the source.
            if (node.Name == "br")
            {
                Append("\n", change);
                return;
            }
            if (node.Name == "pre" && node.ChildNodes.Where(n => n.NodeType != HtmlNodeType.Text
                    || !string.IsNullOrWhiteSpace(n.InnerText)).ToArray() is [var code]
                && code.Name == "code" && code.GetAttributeValue("class", "").Split(' ').Contains("language-json"))
            {
                foreach (var child in code.ChildNodes) Read(child, change);
                return;
            }
            if (!node.HasChildNodes)
            {
                Append(node.OuterHtml, change);
                return;
            }
            // Preserve source tags and attributes as text, while still visiting
            // nested diff wrappers. Never pass any of this HTML to MarkupString.
            var innerStart = node.Closed ? node.InnerStartIndex : node.ChildNodes[0].OuterStartIndex;
            Append(content[node.OuterStartIndex..innerStart], change);
            foreach (var child in node.ChildNodes) Read(child, change);
            // Preserve only a closing tag actually present in the source.
            if (node.EndNode is { } closing && closing != node
                && content.AsSpan(closing.OuterStartIndex).StartsWith("</", StringComparison.Ordinal))
                Append(content.Substring(closing.OuterStartIndex, closing.OuterLength), change);
        }
    }
}
