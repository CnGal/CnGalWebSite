using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Tags;
using CnGalWebSite.APIServer.Application.Videos;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Examines;

/// <summary>对比结果。用户输入导致的失败以错误信息返回，由控制器映射为 400。</summary>
public sealed record RecordComparison<T>(bool Success, T Before, T After, string? Error)
{
    public static RecordComparison<T> Fail(string error) => new(false, default!, default!, error);
}

/// <summary>某条已通过记录的上一条与本条快照；<paramref name="BeforeMainPage"/> 等为原始 Markdown。</summary>
public sealed record RecordFrame(ExaminePreDataModel Before, ExaminePreDataModel After,
    string? BeforeMainPage, string? AfterMainPage);

/// <summary>Request-scoped, immutable projections of approved submissions; never a reconstruction of old database side effects.</summary>
public sealed class RecordHistoryService(AppDbContext db, IEntryService entries, IArticleService articles,
    ITagService tags, IPeripheryService peripheries, IVideoService videos)
{
    private sealed record Timeline(Dictionary<long, object> Views, Dictionary<long, string?> MainPages);

    private readonly Dictionary<(string Kind, long Id), List<Examine>> histories = new();
    private readonly Dictionary<(string Kind, long Id, string Ids), Timeline> timelines = new();

    public static bool Supports(Examine record) => TryGetKey(record, out _);

    /// <summary>
    /// 按通过时间还原 <paramref name="contrastId"/>（0 表示空基准）与 <paramref name="record"/> 的视图。
    /// </summary>
    public async Task<RecordComparison<T>> CompareAsync<T>(Examine record, long contrastId)
    {
        if (!TryGetKey(record, out var key))
        {
            return RecordComparison<T>.Fail("该对象不支持历史回放。");
        }

        var records = await GetApprovedAsync(key);
        var currentIndex = records.FindIndex(s => s.Id == record.Id);
        var contrastIndex = contrastId > 0 ? records.FindIndex(s => s.Id == contrastId) : -1;
        if (currentIndex < 0 || (contrastId > 0 && contrastIndex < 0))
        {
            return RecordComparison<T>.Fail("编辑记录未通过审核");
        }
        if (contrastIndex > currentIndex)
        {
            return RecordComparison<T>.Fail("对比的编辑必须先于当前的编辑");
        }

        var beforeId = contrastId > 0 ? contrastId : 0;
        var timeline = await GetTimelineAsync(key, records, [beforeId, record.Id]);
        if (!timeline.Views.TryGetValue(beforeId, out var before) || !timeline.Views.TryGetValue(record.Id, out var after))
        {
            return RecordComparison<T>.Fail("编辑记录未通过审核");
        }
        return new(true, Clone((T)before), Clone((T)after), null);
    }

    /// <summary>
    /// 获取上一条已通过版本与本条记录的快照；历史不可用时返回 <c>null</c>，由调用方降级显示。
    /// </summary>
    public async Task<RecordFrame?> GetFrameAsync(Examine record)
    {
        if (!TryGetKey(record, out var key))
        {
            return null;
        }

        var records = await GetApprovedAsync(key);
        var index = records.FindIndex(s => s.Id == record.Id);
        if (index < 0)
        {
            return null;
        }
        var beforeId = index == 0 ? 0 : records[index - 1].Id;
        var timeline = await GetTimelineAsync(key, records, [beforeId, record.Id]);
        if (!timeline.Views.TryGetValue(beforeId, out var before) || !timeline.Views.TryGetValue(record.Id, out var after))
        {
            return null;
        }
        return new RecordFrame(Map(before), Map(after),
            timeline.MainPages.GetValueOrDefault(beforeId), timeline.MainPages.GetValueOrDefault(record.Id));
    }

    private async Task<List<Examine>> GetApprovedAsync((string Kind, long Id) key)
    {
        if (histories.TryGetValue(key, out var cached))
        {
            return cached;
        }

        using var read = new RecordReadScope(db);
        var query = db.Examines.AsNoTracking().Where(e => e.IsPassed == true);
        query = key.Kind switch
        {
            "entry" => query.Where(e => (long?)e.EntryId == key.Id),
            "article" => query.Where(e => e.ArticleId == key.Id),
            "tag" => query.Where(e => (long?)e.TagId == key.Id),
            "periphery" => query.Where(e => e.PeripheryId == key.Id),
            _ => query.Where(e => e.VideoId == key.Id)
        };
        var records = RecordVersionOrder.Approved(await query.ToListAsync()).ToList();
        histories.Add(key, records);
        return records;
    }

    private async Task<Timeline> GetTimelineAsync((string Kind, long Id) key, List<Examine> records,
        IReadOnlyCollection<long> projectIds)
    {
        using var read = new RecordReadScope(db);
        // Only project the versions the caller actually needs; applying the rest is cheap and
        // keeps the request from rendering every historical Markdown page.
        var known = records.Select(s => s.Id).ToHashSet();
        var ids = projectIds.Where(id => id == 0 || known.Contains(id)).ToHashSet();
        var cacheKey = (key.Kind, key.Id, string.Join(',', ids.OrderBy(id => id)));
        if (timelines.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var timeline = key.Kind switch
        {
            "entry" => await BuildAsync(new Entry(), records, ids, entries.UpdateEntryDataAsync, ProjectEntryAsync, value => value.MainPage),
            "article" => await BuildAsync(new Article(), records, ids, articles.UpdateArticleData,
                value => Task.FromResult(articles.GetArticleViewModelAsync(value)), value => value.MainPage),
            "tag" => await BuildAsync(new Tag(), records, ids, tags.UpdateTagDataAsync, tags.GetTagViewModel, _ => null),
            "periphery" => await BuildAsync(new Periphery(), records, ids, peripheries.UpdatePeripheryDataAsync,
                value => Task.FromResult(peripheries.GetPeripheryViewModel(value)), _ => null),
            _ => await BuildAsync(new Video(), records, ids, videos.UpdateData,
                value => Task.FromResult(videos.GetViewModel(value)), value => value.MainPage)
        };
        timelines.Add(cacheKey, timeline);
        return timeline;

        async Task<EntryIndexViewModel> ProjectEntryAsync(Entry value)
        {
            var view = await entries.GetEntryIndexViewModelAsync(value);
            view.AllInformation = value.Information?.ToList() ?? [];
            return view;
        }
    }

    private static async Task<Timeline> BuildAsync<T, TView>(T state, List<Examine> records, HashSet<long> ids,
        Func<T, Examine, Task> apply, Func<T, Task<TView>> project, Func<T, string?> mainPage)
    {
        var views = new Dictionary<long, object>();
        var mainPages = new Dictionary<long, string?>();
        await CaptureAsync(0);

        // 记录按通过顺序回放，不能按最大 Id 提前结束：基准版本可能先于当前版本通过、
        // 但 Id 更大，那样当前版本永远不会被回放，合法的对比请求会变成 400。
        var lastPosition = RecordVersionOrder.LastRequestedPosition(records, ids);
        for (var position = 0; position <= lastPosition; position++)
        {
            var record = records[position];
            await apply(state, record);
            await CaptureAsync(record.Id);
        }

        return new Timeline(views, mainPages);

        async Task CaptureAsync(long id)
        {
            if (!ids.Contains(id))
            {
                return;
            }
            views[id] = Clone(await project(state));
            mainPages[id] = mainPage(state);
        }
    }

    private static bool TryGetKey(Examine record, out (string Kind, long Id) key)
    {
        if (record.EntryId is { } entry)
        {
            key = ("entry", entry);
            return true;
        }
        if (record.ArticleId is { } article)
        {
            key = ("article", article);
            return true;
        }
        if (record.TagId is { } tag)
        {
            key = ("tag", tag);
            return true;
        }
        if (record.PeripheryId is { } periphery)
        {
            key = ("periphery", periphery);
            return true;
        }
        if (record.VideoId is { } video)
        {
            key = ("video", video);
            return true;
        }
        key = default;
        return false;
    }

    private static T Clone<T>(T value) => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value))!;

    private static ExaminePreDataModel Map(object view) => view switch
    {
        EntryIndexViewModel value => RecordContentMapper.Map(value),
        ArticleViewModel value => RecordContentMapper.Map(value),
        TagIndexViewModel value => RecordContentMapper.Map(value),
        PeripheryViewModel value => RecordContentMapper.Map(value),
        VideoViewModel value => RecordContentMapper.Map(value),
        _ => throw new InvalidOperationException("未知的历史快照类型。")
    };
}
