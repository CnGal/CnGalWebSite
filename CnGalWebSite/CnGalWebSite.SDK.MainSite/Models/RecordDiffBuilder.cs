using CnGalWebSite.DataModel.ViewModel.Admin;

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
    public static RecordDiff Build(ExaminePreDataModel? before, ExaminePreDataModel? after, bool submissionOnly = false)
    {
        var left = (before?.Items ?? []).Where(Meaningful).ToList();
        var right = (after?.Items ?? []).Where(Meaningful).ToList();
        if (submissionOnly) return new(right.Select(i => new RecordDiffItem(null, i, RecordChangeKind.Submission, false)).ToList());

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
            result.AddRange(oldItems.Select(i => new RecordDiffItem(i, null,
                after?.IsComplete == false ? RecordChangeKind.Unknown : RecordChangeKind.Removed, i.IsKnown)));
            result.AddRange(newItems.Select(i => new RecordDiffItem(null, i,
                before?.IsComplete == true ? RecordChangeKind.Added : RecordChangeKind.Unknown, before?.IsComplete == true)));
        }
        return new(result);
    }

    private static bool Equal(RecordPreviewItem left, RecordPreviewItem right) => left.Kind == right.Kind
        && Normalize(left.Value) == Normalize(right.Value)
        && left.Properties.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => (p.Key, Normalize(p.Value)))
            .SequenceEqual(right.Properties.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => (p.Key, Normalize(p.Value))));

    private static bool Meaningful(RecordPreviewItem item) => !string.IsNullOrEmpty(item.Value)
        || item.Properties.Any(p => !string.IsNullOrEmpty(p.Value));

    private static string Normalize(string? value) => (value ?? "").Replace("\r\n", "\n", StringComparison.Ordinal);
}
