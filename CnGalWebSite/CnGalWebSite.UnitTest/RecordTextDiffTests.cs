using CnGalWebSite.MainSite.Shared.Components.Features.Examine;
using NUnit.Framework;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class RecordTextDiffTests
{
    [Test]
    public void ChangedTextIsSplitIntoRemovedAndAddedSegmentsForEachSide()
    {
        const string beforeText = "这是旧的内容。";
        const string afterText = "这是新的内容。";

        var before = RecordTextDiff.BuildSide(beforeText, afterText, beforeSide: true);
        var after = RecordTextDiff.BuildSide(beforeText, afterText, beforeSide: false);

        Assert.Multiple(() =>
        {
            Assert.That(string.Concat(before.Select(segment => segment.Text)), Is.EqualTo(beforeText));
            Assert.That(string.Concat(after.Select(segment => segment.Text)), Is.EqualTo(afterText));
            Assert.That(before.Any(segment => segment.Change == RecordTextChangeKind.Removed && segment.Text.Contains('旧')), Is.True);
            Assert.That(after.Any(segment => segment.Change == RecordTextChangeKind.Added && segment.Text.Contains('新')), Is.True);
            Assert.That(before.Any(segment => segment.Change == RecordTextChangeKind.Added), Is.False);
            Assert.That(after.Any(segment => segment.Change == RecordTextChangeKind.Removed), Is.False);
        });
    }

    [Test]
    public void AddedTextIsOnlyRenderedOnTheAfterSide()
    {
        var before = RecordTextDiff.BuildSide("abc def", "abc xyz def", beforeSide: true);
        var after = RecordTextDiff.BuildSide("abc def", "abc xyz def", beforeSide: false);

        Assert.Multiple(() =>
        {
            Assert.That(string.Concat(before.Select(segment => segment.Text)), Is.EqualTo("abc def"));
            Assert.That(string.Concat(after.Select(segment => segment.Text)), Is.EqualTo("abc xyz def"));
            Assert.That(before.All(segment => segment.Change == RecordTextChangeKind.Unchanged), Is.True);
            Assert.That(after.Any(segment => segment.Change == RecordTextChangeKind.Added && segment.Text.Contains("xyz")), Is.True);
        });
    }

    [Test]
    public void LongSharedPrefixDoesNotDegradeIntoQuadraticStringCopying()
    {
        var prefix = new string('中', 100_000);
        var before = prefix + "旧";
        var after = prefix + "新";

        var beforeSegments = RecordTextDiff.BuildSide(before, after, beforeSide: true);
        var afterSegments = RecordTextDiff.BuildSide(before, after, beforeSide: false);

        Assert.Multiple(() =>
        {
            Assert.That(string.Concat(beforeSegments.Select(segment => segment.Text)), Is.EqualTo(before));
            Assert.That(string.Concat(afterSegments.Select(segment => segment.Text)), Is.EqualTo(after));
            Assert.That(beforeSegments.Count, Is.LessThanOrEqualTo(2));
            Assert.That(afterSegments.Count, Is.LessThanOrEqualTo(2));
        });
    }

    [Test]
    public void HtmlDiffMarksInsertedAndDeletedContent()
    {
        var diff = RecordHtmlDiff.Build("<p>旧内容</p>", "<p>新内容</p>");
        Assert.That(diff, Does.Contain("<del").And.Contain("<ins"));
        Assert.That(RecordHtmlDiff.Build(string.Empty, "<p>新内容</p>"), Does.StartWith("<ins").And.EndWith("</ins>"));
        Assert.That(RecordHtmlDiff.Build("<p>旧内容</p>", string.Empty), Does.StartWith("<del").And.EndWith("</del>"));
    }

    [Test]
    public void SourceInsAndDelElementsAreNotConfusedWithGeneratedChanges()
    {
        var inserted = RecordHtmlDiff.Build(string.Empty, "<p><del>retained text</del></p>");
        var removed = RecordHtmlDiff.Build("<p><ins>retained text</ins></p>", string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(inserted, Does.Contain("record-diff-inserted"));
            Assert.That(inserted, Does.Contain("<del>retained text</del>"));
            Assert.That(inserted, Does.Not.Contain("record-diff-deleted"));
            Assert.That(removed, Does.Contain("record-diff-deleted"));
            Assert.That(removed, Does.Contain("<ins>retained text</ins>"));
            Assert.That(removed, Does.Not.Contain("record-diff-inserted"));
        });
    }
}
