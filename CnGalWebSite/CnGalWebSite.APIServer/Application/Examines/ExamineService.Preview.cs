using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CnGalWebSite.DataModel.ExamineModel.Comments;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.APIServer.Application.Helper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Tag = CnGalWebSite.DataModel.Model.Tag;

namespace CnGalWebSite.APIServer.Application.Examines;

public partial class ExamineService
{
    public async Task<bool> GetExamineView(ExamineViewModel model, Examine examine)
    {
        var live = await LoadPreviewObject(examine);
        if (live == null) return false;
        model.Type = PreviewType(examine);
        model.ObjectId = ObjectId(examine);
        ApplyObjectSummary(model, live);
        model.EditOverview = HtmlSanitizerHelper.Sanitize(_appHelper.GetJsonStringView(examine.Context));
        model.ContributionValue = examine.ContributionValue;
        model.SubmissionOnly = examine.Operation is Operation.PubulishComment or Operation.RequestUserCertification;
        var records = await PreviewHistory(examine).AsNoTracking().ToListAsync();
        var state = CreatePreviewProjector(live.GetType());

        if (model.SubmissionOnly)
        {
            state.Apply(examine);
            model.BeforeModel = new() { IsComplete = false };
            model.AfterModel = state.Snapshot(RenderPreview);
            if (live is Comment comment)
                await EnrichCommentPreview(examine, model, model.AfterModel);
        }
        else if (examine.IsPassed == null)
        {
            state.Load(live);
            var dependencies = new List<Examine>();
            var visited = new HashSet<long> { examine.Id };
            var dependency = examine.PrepositionExamineId;
            while (dependency is > 0)
            {
                if (!visited.Add(dependency.Value)) { state.IsComplete = false; break; }
                var parent = records.FirstOrDefault(r => r.Id == dependency.Value);
                if (parent == null) { state.IsComplete = false; break; }
                if (parent.IsPassed == true) break;
                if (parent.IsPassed == false) { state.IsComplete = false; break; }
                dependencies.Add(parent);
                dependency = parent.PrepositionExamineId;
            }
            foreach (var parent in dependencies.AsEnumerable().Reverse()) ReplayPreview(state, parent);
            model.BeforeModel = state.Snapshot(RenderPreview);
            ReplayPreview(state, examine);
            model.AfterModel = state.Snapshot(RenderPreview);
        }
        else
        {
            foreach (var record in RecordVersionOrder.Approved(records).Where(r => RecordVersionOrder.Compare(r, examine) < 0))
                ReplayPreview(state, record);
            model.BeforeModel = state.Snapshot(RenderPreview);
            ReplayPreview(state, examine);
            model.AfterModel = state.Snapshot(RenderPreview);
        }
        if (live is PlayedGame playedGame && playedGame.Entry is not null)
            EnrichPlayedGamePreview(playedGame, model.BeforeModel, model.AfterModel);
        await ResolvePreviewReferences(model.BeforeModel, model.AfterModel);
        return true;
    }

    public async Task<EditRecordComparison> GetEditRecordComparison(long contrastId, long currentId, ExaminedNormalListModelType type)
    {
        var selected = await _examineRepository.GetAll().AsNoTracking()
            .Where(r => r.Id == contrastId || r.Id == currentId).ToListAsync();
        var before = selected.FirstOrDefault(r => r.Id == contrastId);
        var after = selected.FirstOrDefault(r => r.Id == currentId);
        if (before == null || after == null || before.IsPassed != true || after.IsPassed != true
            || PreviewType(before) != type || PreviewType(after) != type || ObjectId(before) != ObjectId(after))
            throw new ArgumentException("请选择同一对象的已通过版本。");
        if (RecordVersionOrder.Compare(before, after) > 0)
            throw new ArgumentException("基准版本的通过时间不能晚于目标版本。");
        var records = await PreviewHistory(before).AsNoTracking().Where(r => r.IsPassed == true).ToListAsync();
        var state = CreatePreviewProjector(DomainType(type));
        var result = new EditRecordComparison { ContrastId = contrastId, CurrentId = currentId };
        foreach (var record in RecordVersionOrder.Approved(records))
        {
            if (RecordVersionOrder.Compare(record, after) > 0) break;
            ReplayPreview(state, record);
            if (record.Id == contrastId) result.Before = state.Snapshot(RenderPreview);
            if (record.Id == currentId) result.After = state.Snapshot(RenderPreview);
        }
        await ResolvePreviewReferences(result.Before, result.After);
        return result;
    }

    private RecordPreviewProjector CreatePreviewProjector(Type objectType) =>
        new(objectType, value => _appHelper.GetImagePath(value, ""));

    private void ApplyObjectSummary(ExamineViewModel model, object live)
    {
        model.ObjectName = FirstNonEmpty(Property(live, "DisplayName"), Property(live, "Name"), Property(live, "UserName"));
        model.ObjectBriefIntroduction = FirstNonEmpty(Property(live, "BriefIntroduction"), Property(live, "PersonalSignature"));
        model.Image = _appHelper.GetImagePath(FirstNonEmpty(Property(live, "MainPicture"), Property(live, "MainImage"), Property(live, "PhotoPath")), "app.png");

        switch (live)
        {
            case Entry entry:
                model.ObjectName = FirstNonEmpty(entry.DisplayName, entry.Name);
                if (entry.Type is EntryType.Game or EntryType.ProductionGroup)
                    model.Image = _appHelper.GetImagePath(entry.MainPicture, "app.png");
                else
                {
                    model.Image = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
                    model.IsThumbnail = true;
                }
                break;
            case ApplicationUser user:
                model.ObjectName = FirstNonEmpty(user.UserName);
                model.ObjectBriefIntroduction = user.PersonalSignature;
                model.Image = _appHelper.GetImagePath(user.PhotoPath, "user.png");
                model.IsThumbnail = true;
                break;
            case PlayedGame playedGame when playedGame.Entry is not null:
                model.ObjectName = FirstNonEmpty(playedGame.Entry.DisplayName, playedGame.Entry.Name);
                model.ObjectBriefIntroduction = playedGame.Entry.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(playedGame.Entry.MainPicture, "app.png");
                break;
        }
    }

    private async Task EnrichCommentPreview(Examine examine, ExamineViewModel model, ExaminePreDataModel snapshot)
    {
        CommentText text;
        try
        {
            text = JsonConvert.DeserializeObject<CommentText>(examine.Context);
        }
        catch (JsonException)
        {
            return;
        }
        if (text is null) return;

        var target = await ResolveCommentTarget(text);
        if (target is null) return;
        var (type, id) = target.Value;
        switch (type)
        {
            case CommentType.CommentArticle when long.TryParse(id, out var articleId):
            {
                var article = await _articleRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == articleId);
                if (article is null) return;
                var name = FirstNonEmpty(article.DisplayName, article.Name);
                AddCommentTarget(snapshot, "article", id, name, "/articles/index/" + id);
                model.ObjectName = name;
                model.ObjectBriefIntroduction = article.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(article.MainPicture, "app.png");
                break;
            }
            case CommentType.CommentEntries when int.TryParse(id, out var entryId):
            {
                var entry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == entryId);
                if (entry is null) return;
                var name = FirstNonEmpty(entry.DisplayName, entry.Name);
                AddCommentTarget(snapshot, "entry", id, name, "/entries/index/" + id);
                model.ObjectName = name;
                model.ObjectBriefIntroduction = entry.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(entry.Type is EntryType.Game or EntryType.ProductionGroup ? entry.MainPicture : entry.Thumbnail,
                    entry.Type is EntryType.Game or EntryType.ProductionGroup ? "app.png" : "user.png");
                model.IsThumbnail = entry.Type is not (EntryType.Game or EntryType.ProductionGroup);
                break;
            }
            case CommentType.CommentPeriphery when long.TryParse(id, out var peripheryId):
            {
                var periphery = await _peripheryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == peripheryId);
                if (periphery is null) return;
                var name = FirstNonEmpty(periphery.DisplayName, periphery.Name);
                AddCommentTarget(snapshot, "periphery", id, name, "/peripheries/index/" + id);
                model.ObjectName = name;
                model.ObjectBriefIntroduction = periphery.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(periphery.MainPicture, "app.png");
                break;
            }
            case CommentType.CommentVideo when long.TryParse(id, out var videoId):
            {
                var video = await _videoRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == videoId);
                if (video is null) return;
                var name = FirstNonEmpty(video.DisplayName, video.Name);
                AddCommentTarget(snapshot, "video", id, name, "/videos/index/" + id);
                model.ObjectName = name;
                model.ObjectBriefIntroduction = video.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(video.MainPicture, "app.png");
                break;
            }
            case CommentType.CommentVote when long.TryParse(id, out var voteId):
            {
                var vote = await _voteRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == voteId);
                if (vote is null) return;
                var name = FirstNonEmpty(vote.DisplayName, vote.Name);
                AddCommentTarget(snapshot, "vote", id, name, "/votes/index/" + id);
                model.ObjectName = name;
                model.ObjectBriefIntroduction = vote.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(vote.MainPicture, "app.png");
                break;
            }
            case CommentType.CommentLottery when long.TryParse(id, out var lotteryId):
            {
                var lottery = await _lotteryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == lotteryId);
                if (lottery is null) return;
                var name = FirstNonEmpty(lottery.DisplayName, lottery.Name);
                AddCommentTarget(snapshot, "lottery", id, name, "/lotteries/index/" + id);
                model.ObjectName = name;
                model.ObjectBriefIntroduction = lottery.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(lottery.MainPicture, "app.png");
                break;
            }
            case CommentType.CommentUser:
            {
                var user = await _userRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                if (user is null) return;
                AddCommentTarget(snapshot, "user", id, FirstNonEmpty(user.UserName), "/space/index/" + Uri.EscapeDataString(id));
                model.ObjectName = FirstNonEmpty(user.UserName);
                model.ObjectBriefIntroduction = user.PersonalSignature;
                model.Image = _appHelper.GetImagePath(user.PhotoPath, "user.png");
                model.IsThumbnail = true;
                break;
            }
        }
    }

    private async Task<(CommentType Type, string Id)?> ResolveCommentTarget(CommentText text)
    {
        if (text.Type != CommentType.ReplyComment)
            return string.IsNullOrWhiteSpace(text.ObjectId) ? null : (text.Type, text.ObjectId);

        var objectId = text.ObjectId;
        var visited = new HashSet<long>();
        while (long.TryParse(objectId, out var parentId) && visited.Add(parentId))
        {
            var parent = await _commentRepository.GetAll().AsNoTracking()
                .Include(s => s.ParentCodeNavigation)
                .FirstOrDefaultAsync(s => s.Id == parentId);
            if (parent is null) return null;

            var direct = CommentTarget(parent);
            if (direct is not null) return direct;
            if (parent.Type != CommentType.ReplyComment) return null;
            objectId = parent.ParentCodeNavigation?.Id.ToString();
        }
        return null;
    }

    private static (CommentType Type, string Id)? CommentTarget(Comment comment)
    {
        if (comment.EntryId.HasValue) return (CommentType.CommentEntries, comment.EntryId.Value.ToString());
        if (comment.ArticleId.HasValue) return (CommentType.CommentArticle, comment.ArticleId.Value.ToString());
        if (comment.PeripheryId.HasValue) return (CommentType.CommentPeriphery, comment.PeripheryId.Value.ToString());
        if (comment.VideoId.HasValue) return (CommentType.CommentVideo, comment.VideoId.Value.ToString());
        if (comment.VoteId.HasValue) return (CommentType.CommentVote, comment.VoteId.Value.ToString());
        if (comment.LotteryId.HasValue) return (CommentType.CommentLottery, comment.LotteryId.Value.ToString());
        return null;
    }

    private static void EnrichPlayedGamePreview(PlayedGame playedGame, params ExaminePreDataModel[] snapshots)
    {
        var entry = playedGame.Entry;
        var name = FirstNonEmpty(entry.DisplayName, entry.Name);
        foreach (var snapshot in snapshots)
        {
            if (snapshot.Items.Any(item => item.Kind == RecordPreviewKind.Relation && item.Value == "entry:" + entry.Id)) continue;
            snapshot.Items.Add(new RecordPreviewItem
            {
                Key = "relation/关联词条/entry/" + entry.Id, Group = "关联词条", Kind = RecordPreviewKind.Relation,
                Label = name, Value = "entry:" + entry.Id, DisplayValue = name, Url = "/entries/index/" + entry.Id
            });
        }
    }

    private static void AddCommentTarget(ExaminePreDataModel snapshot, string type, string id, string name, string url)
    {
        snapshot.Items.RemoveAll(i => i.Key == "comment/target");
        snapshot.Items.Add(new RecordPreviewItem
        {
            Key = "comment/target", Group = "评论目标", Kind = RecordPreviewKind.Relation,
            Label = name, Value = type + ":" + id, DisplayValue = name, Url = url
        });
    }

    private void ReplayPreview(RecordPreviewProjector state, Examine record)
    {
        try
        {
            state.Apply(record);
            // The main creation/edit record supplies the initial scalar state. Until then,
            // absent values must not be presented as known empty values.
            if (record.Operation is Operation.EstablishMain or Operation.EditArticleMain or Operation.EditVideoMain
                or Operation.EditPeripheryMain or Operation.EditTagMain or Operation.EditTag or Operation.EditUserMain
                or Operation.EditPlayedGameMain or Operation.EditFavoriteFolderMain or Operation.DisambigMain)
                state.IsComplete = true;
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or FormatException)
        {
            state.IsComplete = false;
        }
    }

    private string RenderPreview(string source) => HtmlSanitizerHelper.Sanitize(_appHelper.MarkdownToHtml(source));

    private IQueryable<Examine> PreviewHistory(Examine record)
    {
        var query = _examineRepository.GetAll();
        return PreviewType(record) switch
        {
            ExaminedNormalListModelType.Entry => query.Where(r => r.EntryId == record.EntryId),
            ExaminedNormalListModelType.Article => query.Where(r => r.ArticleId == record.ArticleId),
            ExaminedNormalListModelType.Tag => query.Where(r => r.TagId == record.TagId),
            ExaminedNormalListModelType.Periphery => query.Where(r => r.PeripheryId == record.PeripheryId),
            ExaminedNormalListModelType.Video => query.Where(r => r.VideoId == record.VideoId),
            ExaminedNormalListModelType.User => query.Where(r => r.ApplicationUserId == record.ApplicationUserId && (r.Operation == Operation.UserMainPage || r.Operation == Operation.EditUserMain)),
            ExaminedNormalListModelType.PlayedGame => query.Where(r => r.PlayedGameId == record.PlayedGameId),
            ExaminedNormalListModelType.FavoriteFolder => query.Where(r => r.FavoriteFolderId == record.FavoriteFolderId),
            ExaminedNormalListModelType.Disambig => query.Where(r => r.DisambigId == record.DisambigId),
            _ => query.Where(r => r.Id == record.Id)
        };
    }

    private async Task<object> LoadPreviewObject(Examine record)
    {
        // Only the editable object is loaded, never tracked navigation graphs.
        switch (PreviewType(record))
        {
            case ExaminedNormalListModelType.Entry:
                return await _entryRepository.GetAll().AsNoTracking().AsSplitQuery()
                    .Include(e => e.Information).ThenInclude(e => e.Additional).Include(e => e.EntryStaffFromEntryNavigation)
                    .Include(e => e.Pictures).Include(e => e.Audio).Include(e => e.Outlinks).Include(e => e.Releases)
                    .Include(e => e.Tags).Include(e => e.Articles).Include(e => e.Videos).Include(e => e.EntryRelationFromEntryNavigation)
                    .Include(e => e.Booking).ThenInclude(b => b.Goals).Include(e => e.WebsiteAddInfor).ThenInclude(w => w.Images)
                    .FirstOrDefaultAsync(e => e.Id == record.EntryId);
            case ExaminedNormalListModelType.Article:
                return await _articleRepository.GetAll().AsNoTracking().AsSplitQuery().Include(e => e.Entries).Include(e => e.Videos)
                    .Include(e => e.Outlinks).Include(e => e.ArticleRelationFromArticleNavigation).FirstOrDefaultAsync(e => e.Id == record.ArticleId);
            case ExaminedNormalListModelType.Tag:
                return await _tagRepository.GetAll().AsNoTracking().AsSplitQuery().Include(e => e.Entries)
                    .Include(e => e.InverseParentCodeNavigation).Include(e => e.ParentCodeNavigation).FirstOrDefaultAsync(e => e.Id == record.TagId);
            case ExaminedNormalListModelType.Periphery:
                return await _peripheryRepository.GetAll().AsNoTracking().AsSplitQuery().Include(e => e.Pictures)
                    .Include(e => e.RelatedEntries).Include(e => e.PeripheryRelationFromPeripheryNavigation).FirstOrDefaultAsync(e => e.Id == record.PeripheryId);
            case ExaminedNormalListModelType.Video:
                return await _videoRepository.GetAll().AsNoTracking().AsSplitQuery().Include(e => e.Pictures).Include(e => e.Outlinks)
                    .Include(e => e.Entries).Include(e => e.Articles).Include(e => e.VideoRelationFromVideoNavigation).FirstOrDefaultAsync(e => e.Id == record.VideoId);
            case ExaminedNormalListModelType.User:
            case ExaminedNormalListModelType.UserCertification:
                return await _userRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(e => e.Id == record.ApplicationUserId);
            case ExaminedNormalListModelType.PlayedGame:
                return await _playedGameRepository.GetAll().AsNoTracking().Include(e => e.Entry)
                    .FirstOrDefaultAsync(e => e.Id == record.PlayedGameId);
            case ExaminedNormalListModelType.FavoriteFolder:
                return await _favoriteFolderRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(e => e.Id == record.FavoriteFolderId);
            case ExaminedNormalListModelType.Disambig:
                return await _disambigRepository.GetAll().AsNoTracking().AsSplitQuery().Include(e => e.Entries).Include(e => e.Articles).FirstOrDefaultAsync(e => e.Id == record.DisambigId);
            default: return await _commentRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(e => e.Id == record.CommentId);
        }
    }

    private async Task ResolvePreviewReferences(params ExaminePreDataModel[] snapshots)
    {
        var items = snapshots.SelectMany(s => s.Items).ToList();
        foreach (var type in new[] { "entry", "article", "video", "tag", "periphery" })
        {
            var references = items.Where(i => i.Kind == RecordPreviewKind.Relation && i.Value?.StartsWith(type + ":", StringComparison.Ordinal) == true).ToList();
            var ids = references.Select(i => long.TryParse(i.Value[(type.Length + 1)..], out var id) ? id : 0).Where(id => id > 0).Distinct().ToList();
            var staff = type == "entry" ? items.Where(i => i.Key.StartsWith("staff/", StringComparison.Ordinal)).ToList() : new List<RecordPreviewItem>();
            ids.AddRange(staff.Select(i => long.TryParse(i.Value.Split(':')[0], out var id) ? id : 0).Where(id => id > 0));
            if (ids.Count == 0) continue;
            Dictionary<long, string> names;
            if (type == "entry") names = (await _entryRepository.GetAll().AsNoTracking().Where(e => ids.Contains(e.Id)).Select(e => new { e.Id, e.Name, e.DisplayName }).ToListAsync()).ToDictionary(e => (long)e.Id, e => e.DisplayName ?? e.Name);
            else if (type == "article") names = (await _articleRepository.GetAll().AsNoTracking().Where(e => ids.Contains(e.Id)).Select(e => new { e.Id, e.Name, e.DisplayName }).ToListAsync()).ToDictionary(e => e.Id, e => e.DisplayName ?? e.Name);
            else if (type == "video") names = (await _videoRepository.GetAll().AsNoTracking().Where(e => ids.Contains(e.Id)).Select(e => new { e.Id, e.Name, e.DisplayName }).ToListAsync()).ToDictionary(e => e.Id, e => e.DisplayName ?? e.Name);
            else if (type == "tag") names = (await _tagRepository.GetAll().AsNoTracking().Where(e => ids.Contains(e.Id)).Select(e => new { e.Id, e.Name }).ToListAsync()).ToDictionary(e => (long)e.Id, e => e.Name);
            else names = (await _peripheryRepository.GetAll().AsNoTracking().Where(e => ids.Contains(e.Id)).Select(e => new { e.Id, e.Name, e.DisplayName }).ToListAsync()).ToDictionary(e => e.Id, e => e.DisplayName ?? e.Name);
            foreach (var item in references)
                if (long.TryParse(item.Value[(type.Length + 1)..], out var id) && names.TryGetValue(id, out var name)) item.DisplayValue = item.Label = name;
            foreach (var item in staff)
                if (long.TryParse(item.Value.Split(':')[0], out var id) && names.TryGetValue(id, out var name)
                    && string.IsNullOrEmpty(item.Properties.FirstOrDefault(p => p.Key == "CustomName")?.Value)) item.DisplayValue = name;
        }
    }

    private static string Property(object value, string name) => value.GetType().GetProperty(name)?.GetValue(value) as string;
    private static string FirstNonEmpty(params string[] values) => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    private static long ObjectId(Examine e) => PreviewType(e) switch
    {
        ExaminedNormalListModelType.Entry => e.EntryId ?? 0, ExaminedNormalListModelType.Article => e.ArticleId ?? 0,
        ExaminedNormalListModelType.Tag => e.TagId ?? 0, ExaminedNormalListModelType.Periphery => e.PeripheryId ?? 0,
        ExaminedNormalListModelType.Video => e.VideoId ?? 0, ExaminedNormalListModelType.PlayedGame => e.PlayedGameId ?? 0,
        ExaminedNormalListModelType.FavoriteFolder => e.FavoriteFolderId ?? 0, ExaminedNormalListModelType.Disambig => e.DisambigId ?? 0,
        _ => e.CommentId ?? 0
    };
    private static ExaminedNormalListModelType PreviewType(Examine e) => e.Operation switch
    {
        Operation.UserMainPage or Operation.EditUserMain => ExaminedNormalListModelType.User,
        Operation.RequestUserCertification => ExaminedNormalListModelType.UserCertification,
        Operation.PubulishComment => ExaminedNormalListModelType.Comment,
        _ when e.EntryId.HasValue => ExaminedNormalListModelType.Entry,
        _ when e.ArticleId.HasValue => ExaminedNormalListModelType.Article,
        _ when e.TagId.HasValue => ExaminedNormalListModelType.Tag,
        _ when e.PeripheryId.HasValue => ExaminedNormalListModelType.Periphery,
        _ when e.VideoId.HasValue => ExaminedNormalListModelType.Video,
        _ when e.PlayedGameId.HasValue => ExaminedNormalListModelType.PlayedGame,
        _ when e.FavoriteFolderId.HasValue => ExaminedNormalListModelType.FavoriteFolder,
        _ => ExaminedNormalListModelType.Disambig
    };
    private static Type DomainType(ExaminedNormalListModelType type) => type switch
    {
        ExaminedNormalListModelType.Entry => typeof(Entry), ExaminedNormalListModelType.Article => typeof(Article),
        ExaminedNormalListModelType.Tag => typeof(Tag), ExaminedNormalListModelType.Periphery => typeof(Periphery),
        ExaminedNormalListModelType.Video => typeof(Video), _ => throw new ArgumentException("该类型不支持历史版本对比。")
    };
}
