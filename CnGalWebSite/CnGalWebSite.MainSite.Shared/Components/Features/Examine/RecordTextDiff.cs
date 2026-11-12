using System.Text;

namespace CnGalWebSite.MainSite.Shared.Components.Features.Examine;

public enum RecordTextChangeKind { Unchanged, Removed, Added }

public sealed record RecordTextSegment(string Text, RecordTextChangeKind Change);

public static class RecordTextDiff
{
    private const int MaxLcsCells = 1_500_000;

    public static IReadOnlyList<RecordTextSegment> BuildSide(string? before, string? after, bool beforeSide)
    {
        before = Normalize(before ?? string.Empty);
        after = Normalize(after ?? string.Empty);
        if (string.Equals(before, after, StringComparison.Ordinal))
            return SplitSide([new(before, RecordTextChangeKind.Unchanged)], beforeSide);

        var beforeTokens = Tokenize(before);
        var afterTokens = Tokenize(after);
        var all = new List<RecordTextSegment>();

        var prefix = 0;
        while (prefix < beforeTokens.Count && prefix < afterTokens.Count
            && string.Equals(beforeTokens[prefix], afterTokens[prefix], StringComparison.Ordinal))
        {
            Append(all, beforeTokens[prefix++], RecordTextChangeKind.Unchanged);
        }

        var suffix = 0;
        while (suffix < beforeTokens.Count - prefix && suffix < afterTokens.Count - prefix
            && string.Equals(beforeTokens[beforeTokens.Count - 1 - suffix], afterTokens[afterTokens.Count - 1 - suffix], StringComparison.Ordinal))
        {
            suffix++;
        }

        var middleBefore = beforeTokens.Skip(prefix).Take(beforeTokens.Count - prefix - suffix).ToList();
        var middleAfter = afterTokens.Skip(prefix).Take(afterTokens.Count - prefix - suffix).ToList();
        AppendLcsDiff(all, middleBefore, middleAfter);
        for (var i = suffix; i > 0; i--)
            Append(all, beforeTokens[beforeTokens.Count - i], RecordTextChangeKind.Unchanged);

        return SplitSide(all, beforeSide);
    }

    private static IReadOnlyList<RecordTextSegment> SplitSide(IEnumerable<RecordTextSegment> segments, bool beforeSide) =>
        Coalesce(segments.Where(segment => beforeSide
            ? segment.Change != RecordTextChangeKind.Added
            : segment.Change != RecordTextChangeKind.Removed));

    private static IReadOnlyList<RecordTextSegment> Coalesce(IEnumerable<RecordTextSegment> segments)
    {
        var result = new List<RecordTextSegment>();
        var text = new StringBuilder();
        RecordTextChangeKind? change = null;
        foreach (var segment in segments)
        {
            if (string.IsNullOrEmpty(segment.Text)) continue;
            if (change is not null && change != segment.Change && text.Length > 0)
            {
                result.Add(new(text.ToString(), change.Value));
                text.Clear();
            }
            change = segment.Change;
            text.Append(segment.Text);
        }
        if (text.Length > 0 && change is not null)
            result.Add(new(text.ToString(), change.Value));
        return result;
    }

    private static void AppendLcsDiff(List<RecordTextSegment> result, List<string> before, List<string> after)
    {
        if ((long)before.Count * after.Count > MaxLcsCells)
        {
            foreach (var token in before) Append(result, token, RecordTextChangeKind.Removed);
            foreach (var token in after) Append(result, token, RecordTextChangeKind.Added);
            return;
        }

        var lengths = new int[before.Count + 1, after.Count + 1];
        for (var i = before.Count - 1; i >= 0; i--)
        {
            for (var j = after.Count - 1; j >= 0; j--)
            {
                lengths[i, j] = string.Equals(before[i], after[j], StringComparison.Ordinal)
                    ? lengths[i + 1, j + 1] + 1
                    : Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
            }
        }

        var left = 0;
        var right = 0;
        while (left < before.Count && right < after.Count)
        {
            if (string.Equals(before[left], after[right], StringComparison.Ordinal))
            {
                Append(result, before[left++], RecordTextChangeKind.Unchanged);
                right++;
            }
            else if (lengths[left + 1, right] >= lengths[left, right + 1])
            {
                Append(result, before[left++], RecordTextChangeKind.Removed);
            }
            else
            {
                Append(result, after[right++], RecordTextChangeKind.Added);
            }
        }
        while (left < before.Count) Append(result, before[left++], RecordTextChangeKind.Removed);
        while (right < after.Count) Append(result, after[right++], RecordTextChangeKind.Added);
    }

    private static void Append(List<RecordTextSegment> result, string text, RecordTextChangeKind change)
    {
        if (!string.IsNullOrEmpty(text)) result.Add(new(text, change));
    }

    private static string Normalize(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    private static List<string> Tokenize(string text)
    {
        var result = new List<string>();
        var index = 0;
        while (index < text.Length)
        {
            var current = text[index];
            if (char.IsWhiteSpace(current))
            {
                var start = index++;
                while (index < text.Length && char.IsWhiteSpace(text[index])) index++;
                result.Add(text[start..index]);
            }
            else if (IsAsciiWordCharacter(current))
            {
                var start = index++;
                while (index < text.Length && IsAsciiWordCharacter(text[index])) index++;
                result.Add(text[start..index]);
            }
            else if (char.IsHighSurrogate(current) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
            {
                result.Add(text.Substring(index, 2));
                index += 2;
            }
            else
            {
                result.Add(current.ToString());
                index++;
            }
        }
        return result;
    }

    private static bool IsAsciiWordCharacter(char value) =>
        value is >= '0' and <= '9' or >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
}
