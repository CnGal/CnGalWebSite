using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using NUnit.Framework;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class ExamineRecordTypeMetadataTests
{
    [Test]
    public void EveryExaminedTypeHasOneConsistentDescriptor()
    {
        foreach (var type in Enum.GetValues<ExaminedNormalListModelType>())
        {
            var descriptor = ExamineRecordTypeMetadata.Get(type);
            Assert.Multiple(() =>
            {
                Assert.That(descriptor.Type, Is.EqualTo(type));
                Assert.That(descriptor.DomainType, Is.Not.Null);
                Assert.That(descriptor.SupportsHistory,
                    Is.EqualTo(descriptor.ApiSegment is not null && descriptor.ObjectPathPrefix is not null));
            });
        }
    }

    [Test]
    public void HistoricalTypesExposeApiSegmentDomainTypeAndObjectUrl()
    {
        var historicalTypes = new[]
        {
            ExaminedNormalListModelType.Entry,
            ExaminedNormalListModelType.Article,
            ExaminedNormalListModelType.Tag,
            ExaminedNormalListModelType.Periphery,
            ExaminedNormalListModelType.Video,
        };

        Assert.That(Enum.GetValues<ExaminedNormalListModelType>().Where(ExamineRecordTypeMetadata.SupportsHistory),
            Is.EquivalentTo(historicalTypes));

        foreach (var type in historicalTypes)
        {
            var descriptor = ExamineRecordTypeMetadata.Get(type);
            Assert.Multiple(() =>
            {
                Assert.That(descriptor.ApiSegment, Is.Not.Null.And.Not.Empty);
                Assert.That(descriptor.DomainType, Is.Not.Null);
                Assert.That(ExamineRecordTypeMetadata.ObjectUrl(type, 42), Is.EqualTo(descriptor.ObjectPathPrefix + "42"));
            });
        }
    }

    [Test]
    public void UserObjectUrlRequiresAndEscapesUserId()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ExamineRecordTypeMetadata.ObjectUrl(ExaminedNormalListModelType.User, 0, "user/a"),
                Is.EqualTo("/space/index/user%2Fa"));
            Assert.That(ExamineRecordTypeMetadata.ObjectUrl(ExaminedNormalListModelType.User, 0), Is.Null);
            Assert.That(ExamineRecordTypeMetadata.ObjectUrl(ExaminedNormalListModelType.Comment, 42), Is.Null);
        });
    }

    [Test]
    public void OperationMetadataMatchesResolvedObjectType()
    {
        var entry = new Examine { Operation = Operation.EditArticleMain, EntryId = 1 };
        var article = new Examine { Operation = Operation.EditArticleMain, ArticleId = 2 };
        var comment = new Examine { Operation = Operation.PubulishComment, CommentId = 3 };

        Assert.Multiple(() =>
        {
            Assert.That(ExamineRecordTypeMetadata.ResolveType(entry), Is.EqualTo(ExaminedNormalListModelType.Entry));
            Assert.That(ExamineRecordTypeMetadata.ResolveType(article), Is.EqualTo(ExaminedNormalListModelType.Article));
            Assert.That(ExamineRecordTypeMetadata.ResolveType(comment), Is.EqualTo(ExaminedNormalListModelType.Comment));
            Assert.That(ExamineRecordTypeMetadata.ResolveObjectId(entry), Is.EqualTo(1));
            Assert.That(ExamineRecordTypeMetadata.ResolveObjectId(article), Is.EqualTo(2));
            Assert.That(ExamineOperationMetadata.IsSubmissionOnly(Operation.PubulishComment), Is.True);
            Assert.That(ExamineOperationMetadata.IsPlainTextContext(Operation.UserMainPage), Is.True);
        });
    }
}
