using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.VideoEdit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class VideoCommandService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<VideoCommandService> logger) : CommandServiceBase(httpClient), IVideoCommandService
{
    protected override ILogger Logger => logger;

    public Task<SdkResult<VideoEditViewModel>> GetVideoCreateTemplateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new VideoEditViewModel
            {
                IsCreate = true,
                Data = new CreateVideoViewModel
                {
                    Main = new EditVideoMainViewModel
                    {
                        PubishTime = DateTime.Now
                    }
                }
            };

            return Task.FromResult(SdkResult<VideoEditViewModel>.Ok(model));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取视频创建模板异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return Task.FromResult(SdkResult<VideoEditViewModel>.Fail("VIDEO_CREATE_TEMPLATE_EXCEPTION", "请求视频创建模板时发生异常"));
        }
    }

    public async Task<SdkResult<VideoEditViewModel>> GetVideoEditAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var mainTask = GetFromJsonAsync<EditVideoMainViewModel>($"api/videos/EditMain/{id}", cancellationToken);
            var imagesTask = GetFromJsonAsync<EditVideoImagesViewModel>($"api/videos/EditImages/{id}", cancellationToken);
            var relevancesTask = GetFromJsonAsync<EditVideoRelevancesViewModel>($"api/videos/EditRelevances/{id}", cancellationToken);
            var mainPageTask = GetFromJsonAsync<EditVideoMainPageViewModel>($"api/videos/EditMainPage/{id}", cancellationToken);

            await Task.WhenAll(mainTask, imagesTask, relevancesTask, mainPageTask);

            var model = new VideoEditViewModel
            {
                IsCreate = false,
                Data = new CreateVideoViewModel
                {
                    Main = mainTask.Result ?? new(),
                    Images = imagesTask.Result ?? new(),
                    Relevances = relevancesTask.Result ?? new(),
                    MainPage = mainPageTask.Result ?? new()
                }
            };

            // 设置 Id 以便后续提交
            model.Data.Id = id;
            model.Data.Name = model.Data.Main.Name;
            model.Data.Main.Id = id;
            model.Data.Images.Id = id;
            model.Data.Relevances.Id = id;
            model.Data.MainPage.Id = id;
            model.Data.MainPage.Name = model.Data.Main.Name;
            model.Data.Relevances.Name = model.Data.Main.Name;
            model.Data.Images.Name = model.Data.Main.Name;

            return SdkResult<VideoEditViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取视频编辑数据异常。VideoId={VideoId}; BaseAddress={BaseAddress}", id, HttpClient.BaseAddress);
            return SdkResult<VideoEditViewModel>.Fail("VIDEO_EDIT_QUERY_EXCEPTION", "请求视频编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<VideoEditMetaOptions>> GetVideoEditMetaOptionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var gamesTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/0", cancellationToken);
            var rolesTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/1", cancellationToken);
            var groupsTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/2", cancellationToken);
            var staffsTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/3", cancellationToken);
            var articlesTask = GetFromJsonAsync<List<string>>("api/articles/GetAllArticles", cancellationToken);
            var videosTask = GetFromJsonAsync<List<string>>("api/videos/GetNames", cancellationToken);

            await Task.WhenAll(gamesTask, rolesTask, groupsTask, staffsTask, articlesTask, videosTask);

            var model = new VideoEditMetaOptions
            {
                EntryGameItems = gamesTask.Result ?? [],
                EntryRoleItems = rolesTask.Result ?? [],
                EntryGroupItems = groupsTask.Result ?? [],
                EntryStaffItems = staffsTask.Result ?? [],
                ArticleItems = articlesTask.Result ?? [],
                VideoItems = videosTask.Result ?? []
            };

            return SdkResult<VideoEditMetaOptions>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取视频候选列表数据异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<VideoEditMetaOptions>.Fail("VIDEO_META_OPTIONS_EXCEPTION", "请求视频候选列表数据时发生异常");
        }
    }

    public async Task<SdkResult<long>> SubmitEditAsync(VideoEditRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.IsCreate)
            {
                var response = await PostAsJsonRawAsync("api/videos/Create", request.Data, cancellationToken);
                var result = await ReadResponseAsync<Result>(response, cancellationToken);

                if (result?.Successful == true && long.TryParse(result.Error, out var newId))
                {
                    InvalidateVideoCaches(newId);
                    return SdkResult<long>.Ok(newId);
                }

                return SdkResult<long>.Fail("VIDEO_CREATE_FAILED", result?.Error ?? "创建视频失败");
            }
            else
            {
                var errors = new List<string>();

                var responses = await Task.WhenAll(
                    SubmitPartAsync("api/videos/EditMain", request.Data.Main, cancellationToken),
                    SubmitPartAsync("api/videos/EditImages", request.Data.Images, cancellationToken),
                    SubmitPartAsync("api/videos/EditRelevances", request.Data.Relevances, cancellationToken),
                    SubmitPartAsync("api/videos/EditMainPage", request.Data.MainPage, cancellationToken)
                );

                foreach (var res in responses)
                {
                    if (res is { Successful: false })
                    {
                        errors.Add(res.Error ?? "某个部分保存失败");
                    }
                }

                if (errors.Count > 0)
                {
                    return SdkResult<long>.Fail("VIDEO_EDIT_PARTIAL_FAILED", string.Join("；", errors));
                }

                InvalidateVideoCaches(request.Data.Main.Id);
                return SdkResult<long>.Ok(request.Data.Main.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "提交视频编辑数据异常。IsCreate={IsCreate}; BaseAddress={BaseAddress}", request.IsCreate, HttpClient.BaseAddress);
            return SdkResult<long>.Fail("VIDEO_SUBMIT_EXCEPTION", "提交视频数据时发生异常");
        }
    }

    private Task<Result?> SubmitPartAsync<T>(string path, T model, CancellationToken cancellationToken)
    {
        return PostAsJsonAsync<T, Result>(path, model, cancellationToken);
    }

    private void InvalidateVideoCaches(long videoId)
    {
        memoryCache.Remove($"main-site:video-detail:{videoId}");
        memoryCache.Remove($"main-site:video-edit-records:{videoId}");
        memoryCache.Remove("main-site:user-content-center");
        memoryCache.Remove("main-site:home-summary");
    }
}
