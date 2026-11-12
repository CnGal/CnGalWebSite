namespace CnGalWebSite.SDK.MainSite.Models;

public enum RecordChangeKind { Unchanged, Added, Removed, Modified, Unknown, Submission }

public sealed record RecordDiffItem(RecordPreviewItem? Before, RecordPreviewItem? After,
    RecordChangeKind Change, bool BeforeKnown = true)
{
    public RecordPreviewItem Item => After ?? Before!;
    public bool Changed => Change != RecordChangeKind.Unchanged;
}

public sealed record RecordDiff(IReadOnlyList<RecordDiffItem> Items)
{
    public bool HasChanges => Items.Any(i => i.Changed);
}

public static class RecordDiffBuilder
{
    public static RecordDiff Build(RecordPreviewModel? before, RecordPreviewModel? after, bool submissionOnly = false)
    {
        var left = (before?.Items ?? []).ToList();
        var right = (after?.Items ?? []).ToList();
        if (submissionOnly) return new(right.Where(Meaningful).Select(i => new RecordDiffItem(null, i, RecordChangeKind.Submission, false)).ToList());

        var result = new List<RecordDiffItem>();
        foreach (var key in left.Select(i => i.Key).Concat(right.Select(i => i.Key)).Distinct(StringComparer.Ordinal))
        {
            var oldItems = left.Where(i => i.Key == key).ToList();
            var newItems = right.Where(i => i.Key == key).ToList();
            foreach (var old in oldItems.ToArray())
            {
                var exact = newItems.FindIndex(i => Equal(old, i));
                if (exact < 0) continue;
                result.Add(new(old, newItems[exact], RecordChangeKind.Unchanged, old.IsKnown));
                oldItems.Remove(old);
                newItems.RemoveAt(exact);
            }
            if (oldItems.Count == 1 && newItems.Count == 1)
            {
                result.Add(new(oldItems[0], newItems[0], RecordChangeKind.Modified, oldItems[0].IsKnown));
                continue;
            }
            result.AddRange(oldItems.Where(Meaningful).Select(i => new RecordDiffItem(i, null,
                after?.IsComplete == true ? RecordChangeKind.Removed : RecordChangeKind.Unknown, i.IsKnown)));
            result.AddRange(newItems.Where(Meaningful).Select(i => new RecordDiffItem(null, i,
                before?.IsComplete == true ? RecordChangeKind.Added : RecordChangeKind.Unknown, before?.IsComplete == true)));
        }
        return new(PairImageReplacements(result));
    }

    private static List<RecordDiffItem> PairImageReplacements(List<RecordDiffItem> items)
    {
        var consumedAdded = new HashSet<int>();
        var result = new List<RecordDiffItem>(items.Count);
        for (var index = 0; index < items.Count; index++)
        {
            if (consumedAdded.Contains(index)) continue;
            var removed = items[index];
            if (removed.Change != RecordChangeKind.Removed || removed.Before?.Kind != RecordPreviewKind.Image)
            {
                result.Add(removed);
                continue;
            }

            var replacementIndex = FindImageReplacement(items, index, consumedAdded);
            if (replacementIndex < 0)
            {
                result.Add(removed);
                continue;
            }

            consumedAdded.Add(replacementIndex);
            var added = items[replacementIndex];
            result.Add(new RecordDiffItem(removed.Before, added.After, RecordChangeKind.Modified,
                removed.BeforeKnown && added.BeforeKnown));
        }
        return result;
    }

    private static int FindImageReplacement(List<RecordDiffItem> items, int removedIndex, HashSet<int> consumed)
    {
        var removed = items[removedIndex].Before!;
        var candidates = Enumerable.Range(0, items.Count)
            .Where(index => !consumed.Contains(index))
            .Where(index => items[index].Change == RecordChangeKind.Added
                && items[index].After?.Kind == RecordPreviewKind.Image
                && string.Equals(items[index].After!.Group, removed.Group, StringComparison.Ordinal))
            .ToList();
        if (candidates.Count == 0) return -1;
        var sameLabel = candidates.Where(index => string.Equals(Normalize(items[index].After!.Label), Normalize(removed.Label), StringComparison.Ordinal)).ToList();
        if (sameLabel.Count == 1) return sameLabel[0];
        if (sameLabel.Count > 1) return -1;
        return candidates.Count == 1 ? candidates[0] : -1;
    }

    private static bool Equal(RecordPreviewItem left, RecordPreviewItem right) => left.Kind == right.Kind
        && Normalize(left.Group) == Normalize(right.Group)
        && Normalize(left.Label) == Normalize(right.Label)
        && Normalize(left.Value) == Normalize(right.Value)
        && Normalize(left.DisplayValue) == Normalize(right.DisplayValue)
        && Normalize(left.Url) == Normalize(right.Url)
        && Normalize(left.Html) == Normalize(right.Html)
        && PropertiesEqual(left.Properties, right.Properties);

    private static bool PropertiesEqual(
        IEnumerable<RecordPreviewProperty> left, IEnumerable<RecordPreviewProperty> right) =>
        left.OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => (p.Key, p.Label, Normalize(p.Value), Normalize(p.DisplayValue)))
            .SequenceEqual(right.OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => (p.Key, p.Label, Normalize(p.Value), Normalize(p.DisplayValue))));

    private static bool Meaningful(RecordPreviewItem item) => !string.IsNullOrEmpty(item.Value)
        || item.Properties.Any(p => !string.IsNullOrEmpty(p.Value));

    private static string Normalize(string? value) => (value ?? "").Replace("\r\n", "\n", StringComparison.Ordinal);
}
