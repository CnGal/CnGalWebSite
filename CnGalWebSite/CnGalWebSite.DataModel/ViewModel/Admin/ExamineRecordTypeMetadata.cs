using System;
using System.Collections.Generic;
using System.Linq;
using CnGalWebSite.DataModel.Model;
using DisambigModel = CnGalWebSite.DataModel.Model.Disambig;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    /// <summary>
    /// 审核对象类型的单一能力描述。API 路由、历史能力和对象链接均从这里派生，
    /// 避免 SDK、服务端和 UI 各自维护一份 switch。
    /// </summary>
    public sealed record ExamineRecordTypeDescriptor(
        ExaminedNormalListModelType Type,
        Type DomainType,
        string ApiSegment,
        string ObjectPathPrefix)
    {
        public bool SupportsHistory => DomainType is not null && ApiSegment is not null && ObjectPathPrefix is not null;
    }

    public static class ExamineRecordTypeMetadata
    {
        private static readonly IReadOnlyDictionary<ExaminedNormalListModelType, ExamineRecordTypeDescriptor> Descriptors =
            new[]
            {
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.Entry, typeof(Entry), "entries", "/entries/index/"),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.Article, typeof(Article), "articles", "/articles/index/"),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.Tag, typeof(Tag), "tags", "/tags/index/"),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.Periphery, typeof(Periphery), "peripheries", "/peripheries/index/"),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.Video, typeof(Video), "videos", "/videos/index/"),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.FavoriteFolder, typeof(FavoriteFolder), null, "/favoritefolders/index/"),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.PlayedGame, typeof(PlayedGame), null, null),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.Disambig, typeof(DisambigModel), null, null),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.Comment, typeof(Comment), null, null),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.User, typeof(ApplicationUser), null, "/space/index/"),
                new ExamineRecordTypeDescriptor(ExaminedNormalListModelType.UserCertification, typeof(ApplicationUser), null, "/space/index/"),
            }.ToDictionary(item => item.Type);

        public static ExamineRecordTypeDescriptor Get(ExaminedNormalListModelType type) => Descriptors[type];

        public static string ApiSegment(ExaminedNormalListModelType type) =>
            Descriptors.TryGetValue(type, out var descriptor) ? descriptor.ApiSegment : null;

        public static Type DomainType(ExaminedNormalListModelType type) =>
            Descriptors.TryGetValue(type, out var descriptor) ? descriptor.DomainType : null;

        public static bool SupportsHistory(ExaminedNormalListModelType type) =>
            Descriptors.TryGetValue(type, out var descriptor) && descriptor.SupportsHistory;

        public static string ObjectUrl(ExaminedNormalListModelType type, long id, string userId = null)
        {
            if (!Descriptors.TryGetValue(type, out var descriptor) || descriptor.ObjectPathPrefix is null)
            {
                return null;
            }

            if (type is ExaminedNormalListModelType.User or ExaminedNormalListModelType.UserCertification)
            {
                return string.IsNullOrWhiteSpace(userId)
                    ? null
                    : descriptor.ObjectPathPrefix + Uri.EscapeDataString(userId);
            }

            return id > 0 ? descriptor.ObjectPathPrefix + id : null;
        }

        /// <summary>
        /// 从审核记录推断所属对象类型。操作本身的语义优先于外键，避免评论等操作被误判。
        /// </summary>
        public static ExaminedNormalListModelType ResolveType(Examine examine) => examine.Operation switch
        {
            Operation.UserMainPage or Operation.EditUserMain => ExaminedNormalListModelType.User,
            Operation.RequestUserCertification => ExaminedNormalListModelType.UserCertification,
            Operation.PubulishComment => ExaminedNormalListModelType.Comment,
            _ when examine.EntryId.HasValue => ExaminedNormalListModelType.Entry,
            _ when examine.ArticleId.HasValue => ExaminedNormalListModelType.Article,
            _ when examine.TagId.HasValue => ExaminedNormalListModelType.Tag,
            _ when examine.PeripheryId.HasValue => ExaminedNormalListModelType.Periphery,
            _ when examine.VideoId.HasValue => ExaminedNormalListModelType.Video,
            _ when examine.PlayedGameId.HasValue => ExaminedNormalListModelType.PlayedGame,
            _ when examine.FavoriteFolderId.HasValue => ExaminedNormalListModelType.FavoriteFolder,
            _ => ExaminedNormalListModelType.Disambig
        };

        public static long ResolveObjectId(Examine examine) => ResolveType(examine) switch
        {
            ExaminedNormalListModelType.Entry => examine.EntryId ?? 0,
            ExaminedNormalListModelType.Article => examine.ArticleId ?? 0,
            ExaminedNormalListModelType.Tag => examine.TagId ?? 0,
            ExaminedNormalListModelType.Periphery => examine.PeripheryId ?? 0,
            ExaminedNormalListModelType.Video => examine.VideoId ?? 0,
            ExaminedNormalListModelType.PlayedGame => examine.PlayedGameId ?? 0,
            ExaminedNormalListModelType.FavoriteFolder => examine.FavoriteFolderId ?? 0,
            ExaminedNormalListModelType.Disambig => examine.DisambigId ?? 0,
            _ => 0
        };

        /// <summary>
        /// 将查询限定到同一对象的历史记录，供服务端重建版本时复用。
        /// </summary>
        public static IQueryable<Examine> ScopeHistory(IQueryable<Examine> query, ExaminedNormalListModelType type, Examine record)
        {
            return type switch
            {
                ExaminedNormalListModelType.Entry => query.Where(item => item.EntryId == record.EntryId),
                ExaminedNormalListModelType.Article => query.Where(item => item.ArticleId == record.ArticleId),
                ExaminedNormalListModelType.Tag => query.Where(item => item.TagId == record.TagId),
                ExaminedNormalListModelType.Periphery => query.Where(item => item.PeripheryId == record.PeripheryId),
                ExaminedNormalListModelType.Video => query.Where(item => item.VideoId == record.VideoId),
                ExaminedNormalListModelType.User or ExaminedNormalListModelType.UserCertification =>
                    query.Where(item => item.ApplicationUserId == record.ApplicationUserId
                        && (item.Operation == Operation.UserMainPage || item.Operation == Operation.EditUserMain)),
                ExaminedNormalListModelType.PlayedGame => query.Where(item => item.PlayedGameId == record.PlayedGameId),
                ExaminedNormalListModelType.FavoriteFolder => query.Where(item => item.FavoriteFolderId == record.FavoriteFolderId),
                ExaminedNormalListModelType.Disambig => query.Where(item => item.DisambigId == record.DisambigId),
                _ => query.Where(item => item.Id == record.Id)
            };
        }
    }

    /// <summary>
    /// 审核操作在展示层的统一语义，避免前后端分别维护提交型和纯文本操作列表。
    /// </summary>
    public static class ExamineOperationMetadata
    {
        public static bool IsSubmissionOnly(Operation operation) =>
            operation is Operation.PubulishComment or Operation.RequestUserCertification;

        public static bool IsPlainTextContext(Operation operation) => operation is
            Operation.EstablishMainPage or Operation.EditArticleMainPage or Operation.EditVideoMainPage or Operation.UserMainPage;
    }
}
