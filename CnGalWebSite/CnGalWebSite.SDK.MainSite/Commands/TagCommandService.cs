using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.TagEdit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class TagCommandService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<TagCommandService> logger,
    ExamineUserCache userCache,
    ICurrentUserAccessor currentUserAccessor) : CommandServiceBase(httpClient), ITagCommandService
{
    protected override ILogger Logger => logger;

    public Task<SdkResult<TagEditViewModel>> GetTagCreateTemplateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new TagEditViewModel
            {
                IsCreate = true,
                Data = new CreateTagViewModel()
            };

            return Task.FromResult(SdkResult<TagEditViewModel>.Ok(model));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取标签创建模板异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return Task.FromResult(SdkResult<TagEditViewModel>.Fail("TAG_CREATE_TEMPLATE_EXCEPTION", "请求标签创建模板时发生异常"));
        }
    }

    public async Task<SdkResult<TagEditViewModel>> GetTagEditAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var mainTask = GetFromJsonAsync<EditTagMainViewModel>($"api/tags/editmain/{id}", cancellationToken);
            var entriesTask = GetFromJsonAsync<EditTagChildEntriesViewModel>($"api/tags/EditChildEntries/{id}", cancellationToken);
            var childTagsTask = GetFromJsonAsync<EditTagChildTagsViewModel>($"api/tags/EditChildTags/{id}", cancellationToken);

            await Task.WhenAll(mainTask, entriesTask, childTagsTask);

            var model = new TagEditViewModel
            {
                IsCreate = false,
                Data = new CreateTagViewModel
                {
                    Main = mainTask.Result ?? new(),
                    Entries = entriesTask.Result ?? new(),
                    Tags = childTagsTask.Result ?? new()
                }
            };

            // 从 Main 同步 Id 和 Name 到顶层
            model.Data.Id = id;
            model.Data.Name = model.Data.Main.Name;
            model.Data.Main.Id = id;

            return SdkResult<TagEditViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取标签编辑数据异常。TagId={TagId}; BaseAddress={BaseAddress}", id, HttpClient.BaseAddress);
            return SdkResult<TagEditViewModel>.Fail("TAG_EDIT_QUERY_EXCEPTION", "请求标签编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<TagEditMetaOptions>> GetTagEditMetaOptionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tagsTask = GetFromJsonAsync<List<string>>("api/tags/GetAllTags", cancellationToken);
            var entriesTask = GetFromJsonAsync<List<string>>("api/home/GetSearchTipList", cancellationToken);

            await Task.WhenAll(tagsTask, entriesTask);

            var options = new TagEditMetaOptions
            {
                TagItems = tagsTask.Result ?? [],
                EntryItems = entriesTask.Result ?? []
            };

            return SdkResult<TagEditMetaOptions>.Ok(options);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取标签候选列表数据异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<TagEditMetaOptions>.Fail("TAG_META_OPTIONS_EXCEPTION", "请求标签候选列表数据时发生异常");
        }
    }

    public async Task<SdkResult<long>> SubmitEditAsync(TagEditRequest request, CancellationToken cancellationToken = default)
    {
        var writeAttempted = false;
        string? userId = null;
        long? cacheEntityId = null;
        try
        {
            userId = await currentUserAccessor.GetCurrentUserIdAsync(cancellationToken);
            cacheEntityId = request.IsCreate ? null : request.Data.Main.Id;
            writeAttempted = true;
            if (request.IsCreate)
            {
                var response = await PostAsJsonRawAsync("api/tags/CreateTag", request.Data, cancellationToken);
                var result = await ReadResponseAsync<Result>(response, cancellationToken);

                if (result?.Successful == true && long.TryParse(result.Error, out var newId))
                {
                    cacheEntityId = newId;
                    return SdkResult<long>.Ok(newId);
                }

                return SdkResult<long>.Fail("TAG_CREATE_FAILED", result?.Error ?? "创建标签失败");
            }
            else
            {
                var errors = new List<string>();

                var responses = await Task.WhenAll(
                    SubmitPartAsync("api/tags/EditMain", request.Data.Main, cancellationToken),
                    SubmitPartAsync("api/tags/EditChildEntries", request.Data.Entries, cancellationToken),
                    SubmitPartAsync("api/tags/EditChildTags", request.Data.Tags, cancellationToken)
                );

                foreach (var res in responses)
                {
                    if (res?.Successful != true)
                    {
                        errors.Add(res?.Error ?? "某个部分保存失败");
                    }
                }

                if (errors.Count > 0)
                {
                    return SdkResult<long>.Fail("TAG_EDIT_PARTIAL_FAILED", string.Join("；", errors));
                }

                return SdkResult<long>.Ok(request.Data.Main.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "提交标签编辑数据异常。IsCreate={IsCreate}; BaseAddress={BaseAddress}", request.IsCreate, HttpClient.BaseAddress);
            return SdkResult<long>.Fail("TAG_SUBMIT_EXCEPTION", "提交标签数据时发生异常");
        }
        finally
        {
            // 分块写入可能部分成功，异常或取消时服务端结果也可能不确定。
            if (writeAttempted)
            {
                if (cacheEntityId.HasValue)
                {
                    InvalidateTagCaches(cacheEntityId.Value);
                }
                userCache.Invalidate(HttpClient.BaseAddress, userId);
                userCache.InvalidateReviewQueue(HttpClient.BaseAddress);
            }
        }
    }

    private Task<Result?> SubmitPartAsync<T>(string path, T model, CancellationToken cancellationToken)
    {
        return PostAsJsonAsync<T, Result>(path, model, cancellationToken);
    }

    private void InvalidateTagCaches(long tagId)
    {
        memoryCache.Remove($"main-site:tag-detail:{tagId}");
        memoryCache.Remove("main-site:tag-tree");
        memoryCache.Remove(ExamineCacheKeys.TagEditRecords(tagId));
    }
}
