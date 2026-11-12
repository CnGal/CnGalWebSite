using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        model.ObjectName = Property(live, "DisplayName") ?? Property(live, "Name") ?? Property(live, "UserName");
        model.ObjectBriefIntroduction = Property(live, "BriefIntroduction") ?? Property(live, "PersonalSignature");
        model.Image = _appHelper.GetImagePath(Property(live, "MainPicture") ?? Property(live, "MainImage") ?? Property(live, "PhotoPath"), "app.png");
        model.EditOverview = HtmlSanitizerHelper.Sanitize(_appHelper.GetJsonStringView(examine.Context));
        model.ContributionValue = examine.ContributionValue;
        model.SubmissionOnly = examine.Operation is Operation.PubulishComment or Operation.RequestUserCertification;
        var records = await PreviewHistory(examine).AsNoTracking().ToListAsync();
        var state = new RecordPreviewProjector(live.GetType());

        if (model.SubmissionOnly)
        {
            state.Apply(examine);
            model.BeforeModel = new() { IsComplete = false };
            model.AfterModel = state.Snapshot(RenderPreview);
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
        var state = new RecordPreviewProjector(DomainType(type));
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
                return await _playedGameRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(e => e.Id == record.PlayedGameId);
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
