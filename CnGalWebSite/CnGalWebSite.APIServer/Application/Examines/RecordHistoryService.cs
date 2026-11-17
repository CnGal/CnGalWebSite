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

/// <summary>Request-scoped, immutable projections of approved submissions; never a reconstruction of old database side effects.</summary>
public sealed class RecordHistoryService(AppDbContext db, IEntryService entries, IArticleService articles,
    ITagService tags, IPeripheryService peripheries, IVideoService videos)
{
    private sealed record Timeline(List<long> Order, Dictionary<long, object> Views);
    private readonly Dictionary<(string Kind, long Id), Timeline> timelines = new();

    public static bool Supports(Examine record) => record.EntryId.HasValue || record.ArticleId.HasValue
        || record.TagId.HasValue || record.PeripheryId.HasValue || record.VideoId.HasValue;

    public async Task<(T Before, T After)> CompareAsync<T>(Examine record, long contrastId)
    {
        var timeline = await GetTimelineAsync(record);
        if (!timeline.Views.TryGetValue(contrastId, out var before) || !timeline.Views.TryGetValue(record.Id, out var after)
            || timeline.Order.IndexOf(contrastId) > timeline.Order.IndexOf(record.Id))
            throw new InvalidOperationException("请选择同一对象按通过时间排序的已通过版本。");
        return (Clone((T)before), Clone((T)after));
    }

    public async Task<(ExaminePreDataModel Before, ExaminePreDataModel After)> GetFrameAsync(Examine record)
    {
        var timeline = await GetTimelineAsync(record);
        var index = timeline.Order.IndexOf(record.Id);
        if (index <= 0) throw new InvalidOperationException("无法找到已通过的历史记录。");
        return (Map(timeline.Views[timeline.Order[index - 1]]), Map(timeline.Views[record.Id]));
    }

    private async Task<Timeline> GetTimelineAsync(Examine record)
    {
        using var read = new RecordReadScope(db);
        var key = record.EntryId is { } entry ? ("entry", (long)entry)
            : record.ArticleId is { } article ? ("article", article)
            : record.TagId is { } tag ? ("tag", (long)tag)
            : record.PeripheryId is { } periphery ? ("periphery", periphery)
            : record.VideoId is { } video ? ("video", video)
            : throw new InvalidOperationException("该对象不支持历史回放。");
        if (timelines.TryGetValue(key, out var cached)) return cached;
        var query = db.Examines.AsNoTracking().Where(e => e.IsPassed == true);
        query = key.Item1 switch
        {
            "entry" => query.Where(e => e.EntryId == record.EntryId),
            "article" => query.Where(e => e.ArticleId == record.ArticleId),
            "tag" => query.Where(e => e.TagId == record.TagId),
            "periphery" => query.Where(e => e.PeripheryId == record.PeripheryId),
            _ => query.Where(e => e.VideoId == record.VideoId)
        };
        var records = RecordVersionOrder.Approved(await query.ToListAsync()).ToList();
        var result = key.Item1 switch
        {
            "entry" => await BuildAsync(new Entry(), records, entries.UpdateEntryDataAsync, async value =>
            {
                var view = await entries.GetEntryIndexViewModelAsync(value);
                view.AllInformation = value.Information.ToList();
                return view;
            }),
            "article" => await BuildAsync(new Article(), records, articles.UpdateArticleData,
                value => Task.FromResult(articles.GetArticleViewModelAsync(value))),
            "tag" => await BuildAsync(new Tag(), records, tags.UpdateTagDataAsync, tags.GetTagViewModel),
            "periphery" => await BuildAsync(new Periphery(), records, peripheries.UpdatePeripheryDataAsync,
                value => Task.FromResult(peripheries.GetPeripheryViewModel(value))),
            _ => await BuildAsync(new Video(), records, videos.UpdateData,
                value => Task.FromResult(videos.GetViewModel(value)))
        };
        timelines.Add(key, result);
        return result;
    }

    private static async Task<Timeline> BuildAsync<T, TView>(T state, List<Examine> records,
        Func<T, Examine, Task> apply, Func<T, Task<TView>> project)
    {
        var result = new Timeline([0], new() { [0] = Clone(await project(state)) });
        foreach (var record in records)
        {
            await apply(state, record);
            result.Order.Add(record.Id);
            result.Views.Add(record.Id, Clone(await project(state)));
        }
        return result;
    }

    private static T Clone<T>(T value) => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
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
