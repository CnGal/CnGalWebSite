using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.PeripheryEdit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class PeripheryCommandService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<PeripheryCommandService> logger) : CommandServiceBase(httpClient), IPeripheryCommandService
{
    protected override ILogger Logger => logger;

    public Task<SdkResult<PeripheryEditViewModel>> GetPeripheryCreateTemplateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new PeripheryEditViewModel
            {
                IsCreate = true,
                Data = new CreatePeripheryViewModel()
            };

            return Task.FromResult(SdkResult<PeripheryEditViewModel>.Ok(model));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取周边创建模板异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return Task.FromResult(SdkResult<PeripheryEditViewModel>.Fail("PERIPHERY_CREATE_TEMPLATE_EXCEPTION", "请求周边创建模板时发生异常"));
        }
    }

    public async Task<SdkResult<PeripheryEditViewModel>> GetPeripheryEditAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var mainTask = GetFromJsonAsync<EditPeripheryMainViewModel>($"api/peripheries/editmain/{id}", cancellationToken);
            var imagesTask = GetFromJsonAsync<EditPeripheryImagesViewModel>($"api/peripheries/editimages/{id}", cancellationToken);
            var entriesTask = GetFromJsonAsync<EditPeripheryRelatedEntriesViewModel>($"api/peripheries/editrelatedentries/{id}", cancellationToken);
            var peripheriesTask = GetFromJsonAsync<EditPeripheryRelatedPeripheriesViewModel>($"api/peripheries/editrelatedperipheries/{id}", cancellationToken);

            await Task.WhenAll(mainTask, imagesTask, entriesTask, peripheriesTask);

            var model = new PeripheryEditViewModel
            {
                IsCreate = false,
                Data = new CreatePeripheryViewModel
                {
                    Main = mainTask.Result ?? new(),
                    Images = imagesTask.Result ?? new(),
                    Entries = entriesTask.Result ?? new(),
                    Peripheries = peripheriesTask.Result ?? new()
                }
            };

            // 设置 Id 和 Name 以便后续提交
            model.Data.Id = id;
            model.Data.Name = model.Data.Main.Name;
            model.Data.Main.Id = id;
            model.Data.Images.Id = id;
            model.Data.Entries.Id = id;
            model.Data.Peripheries.Id = id;
            model.Data.Images.Name = model.Data.Main.Name;
            model.Data.Entries.Name = model.Data.Main.Name;
            model.Data.Peripheries.Name = model.Data.Main.Name;

            return SdkResult<PeripheryEditViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取周边编辑数据异常。PeripheryId={PeripheryId}; BaseAddress={BaseAddress}", id, HttpClient.BaseAddress);
            return SdkResult<PeripheryEditViewModel>.Fail("PERIPHERY_EDIT_QUERY_EXCEPTION", "请求周边编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<PeripheryEditMetaOptions>> GetPeripheryEditMetaOptionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var gamesTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/0", cancellationToken);
            var rolesTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/1", cancellationToken);
            var groupsTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/2", cancellationToken);
            var staffsTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/3", cancellationToken);
            var peripheriesTask = GetFromJsonAsync<List<string>>("api/peripheries/GetAllPeripheries", cancellationToken);

            await Task.WhenAll(gamesTask, rolesTask, groupsTask, staffsTask, peripheriesTask);

            var model = new PeripheryEditMetaOptions
            {
                EntryGameItems = gamesTask.Result ?? [],
                EntryRoleItems = rolesTask.Result ?? [],
                EntryGroupItems = groupsTask.Result ?? [],
                EntryStaffItems = staffsTask.Result ?? [],
                PeripheryItems = peripheriesTask.Result ?? []
            };

            return SdkResult<PeripheryEditMetaOptions>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取周边候选列表数据异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<PeripheryEditMetaOptions>.Fail("PERIPHERY_META_OPTIONS_EXCEPTION", "请求周边候选列表数据时发生异常");
        }
    }

    public async Task<SdkResult<long>> SubmitEditAsync(PeripheryEditRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.IsCreate)
            {
                var response = await PostAsJsonRawAsync("api/peripheries/CreatePeriphery", request.Data, cancellationToken);
                var result = await ReadResponseAsync<Result>(response, cancellationToken);

                if (result?.Successful == true && long.TryParse(result.Error, out var newId))
                {
                    InvalidatePeripheryCaches(newId);
                    return SdkResult<long>.Ok(newId);
                }

                return SdkResult<long>.Fail("PERIPHERY_CREATE_FAILED", result?.Error ?? "创建周边失败");
            }
            else
            {
                var errors = new List<string>();

                var responses = await Task.WhenAll(
                    SubmitPartAsync("api/peripheries/EditMain", request.Data.Main, cancellationToken),
                    SubmitPartAsync("api/peripheries/EditImages", request.Data.Images, cancellationToken),
                    SubmitPartAsync("api/peripheries/EditRelatedEntries", request.Data.Entries, cancellationToken),
                    SubmitPartAsync("api/peripheries/EditRelatedPeripheries", request.Data.Peripheries, cancellationToken)
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
                    return SdkResult<long>.Fail("PERIPHERY_EDIT_PARTIAL_FAILED", string.Join("；", errors));
                }

                InvalidatePeripheryCaches(request.Data.Main.Id);
                return SdkResult<long>.Ok(request.Data.Main.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "提交周边编辑数据异常。IsCreate={IsCreate}; BaseAddress={BaseAddress}", request.IsCreate, HttpClient.BaseAddress);
            return SdkResult<long>.Fail("PERIPHERY_SUBMIT_EXCEPTION", "提交周边数据时发生异常");
        }
    }

    private Task<Result?> SubmitPartAsync<T>(string path, T model, CancellationToken cancellationToken)
    {
        return PostAsJsonAsync<T, Result>(path, model, cancellationToken);
    }

    public async Task<SdkResult<bool>> CheckIsCollectedAsync(long peripheryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<CheckPeripheryIsCollectedModel>(
                $"api/peripheries/CheckPeripheryIsCollected/{peripheryId}", cancellationToken);

            return SdkResult<bool>.Ok(result?.IsCollected ?? false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "检查周边收集状态异常。PeripheryId={PeripheryId}; BaseAddress={BaseAddress}", peripheryId, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("PERIPHERY_CHECK_COLLECTED_EXCEPTION", "检查周边收集状态时发生异常");
        }
    }

    public async Task<SdkResult<bool>> CollectAsync(long peripheryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<Result>(
                $"api/peripheries/CollectPeriphery/{peripheryId}", cancellationToken);

            if (result is { Successful: true })
            {
                return SdkResult<bool>.Ok(true);
            }

            return SdkResult<bool>.Fail("PERIPHERY_COLLECT_FAILED", result?.Error ?? "收集周边失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "收集周边异常。PeripheryId={PeripheryId}; BaseAddress={BaseAddress}", peripheryId, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("PERIPHERY_COLLECT_EXCEPTION", "收集周边时发生异常");
        }
    }

    public async Task<SdkResult<bool>> UnCollectAsync(long peripheryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<Result>(
                $"api/peripheries/UnCollectPeriphery/{peripheryId}", cancellationToken);

            if (result is { Successful: true })
            {
                return SdkResult<bool>.Ok(true);
            }

            return SdkResult<bool>.Fail("PERIPHERY_UNCOLLECT_FAILED", result?.Error ?? "取消收集周边失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取消收集周边异常。PeripheryId={PeripheryId}; BaseAddress={BaseAddress}", peripheryId, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("PERIPHERY_UNCOLLECT_EXCEPTION", "取消收集周边时发生异常");
        }
    }

    private void InvalidatePeripheryCaches(long peripheryId)
    {
        memoryCache.Remove($"main-site:periphery-detail:{peripheryId}");
        memoryCache.Remove($"main-site:periphery-edit-records:{peripheryId}");
        memoryCache.Remove("main-site:user-content-center");
    }
}
