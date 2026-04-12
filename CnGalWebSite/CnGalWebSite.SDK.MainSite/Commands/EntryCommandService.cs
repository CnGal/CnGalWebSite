using System.Text.Json;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.EntryEdit;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class EntryCommandService(
    HttpClient httpClient,
    ILogger<EntryCommandService> logger) : CommandServiceBase(httpClient), IEntryCommandService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<EntryEditViewModel>> GetEntryCreateTemplateAsync(EntryType type = EntryType.Game, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new EntryEditViewModel
            {
                IsCreate = true,
                Data = new EstablishEntryViewModel
                {
                    Type = type,
                    Main = { Type = type },
                    AddInfor = { Type = type },
                    Relevances = { Type = type }
                }
            };
            
            // 获取并初始化各个类型的基础信息模板
            var informations = await GetFromJsonAsync<List<EditInformationModel>>($"api/entries/GetEditInformationModelList?type={(int)type}", cancellationToken);
            if (informations != null)
            {
                foreach (var item in informations)
                {
                    item.Value = null;
                }
                model.Data.AddInfor.Informations = informations;
            }

            return SdkResult<EntryEditViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取词条创建模板异常。Type={Type}; BaseAddress={BaseAddress}", type, HttpClient.BaseAddress);
            return SdkResult<EntryEditViewModel>.Fail("ENTRY_CREATE_TEMPLATE_EXCEPTION", "请求词条创建模板时发生异常");
        }
    }

    public async Task<SdkResult<EntryEditViewModel>> GetEntryEditAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            // 并发获取所有编辑部分的当前数据
            var mainTask = GetFromJsonAsync<EditMainViewModel>($"api/entries/EditMain/{id}", cancellationToken);
            var addInforTask = GetFromJsonAsync<EditAddInforViewModel>($"api/entries/EditAddInfor/{id}", cancellationToken);
            var mainPageTask = GetFromJsonAsync<EditMainPageViewModel>($"api/entries/EditMainPage/{id}", cancellationToken);
            var imagesTask = GetFromJsonAsync<EditImagesViewModel>($"api/entries/EditImages/{id}", cancellationToken);
            var relevancesTask = GetFromJsonAsync<EditRelevancesViewModel>($"api/entries/EditRelevances/{id}", cancellationToken);
            var tagsTask = GetFromJsonAsync<EditEntryTagViewModel>($"api/entries/EditTags/{id}", cancellationToken);
            var audioTask = GetFromJsonAsync<EditAudioViewModel>($"api/entries/EditAudio/{id}", cancellationToken);
            var websiteTask = GetFromJsonAsync<EditEntryWebsiteViewModel>($"api/entries/EditWebsite/{id}", cancellationToken);

            await Task.WhenAll(mainTask, addInforTask, mainPageTask, imagesTask, relevancesTask, tagsTask, audioTask, websiteTask);

            var model = new EntryEditViewModel
            {
                IsCreate = false,
                Data = new EstablishEntryViewModel
                {
                    Main = mainTask.Result ?? new(),
                    AddInfor = addInforTask.Result ?? new(),
                    MainPage = mainPageTask.Result ?? new(),
                    Images = imagesTask.Result ?? new(),
                    Relevances = relevancesTask.Result ?? new(),
                    Tags = tagsTask.Result ?? new(),
                    Audio = audioTask.Result ?? new(),
                    Website = websiteTask.Result ?? new()
                }
            };
            
            // 确保类型统一
            model.Data.Type = model.Data.Main.Type;
            model.Data.Id = id;
            model.Data.Name = model.Data.Main.Name;
            
            return SdkResult<EntryEditViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取词条编辑数据异常。EntryId={EntryId}; BaseAddress={BaseAddress}", id, HttpClient.BaseAddress);
            return SdkResult<EntryEditViewModel>.Fail("ENTRY_EDIT_QUERY_EXCEPTION", "请求词条编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<EntryEditMetaOptions>> GetEntryEditMetaOptionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var gamesTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/0", cancellationToken);
            var rolesTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/1", cancellationToken);
            var groupsTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/2", cancellationToken);
            var staffsTask = GetFromJsonAsync<List<string>>("api/entries/GetAllEntries/3", cancellationToken);
            var articlesTask = GetFromJsonAsync<List<string>>("api/articles/GetAllArticles", cancellationToken);
            var tagsTask = GetFromJsonAsync<List<string>>("api/tags/GetAllTags", cancellationToken);
            var videosTask = GetFromJsonAsync<List<string>>("api/videos/GetNames", cancellationToken);
            var lotteriesTask = GetFromJsonAsync<List<string>>("api/lotteries/GetNames", cancellationToken);

            await Task.WhenAll(gamesTask, rolesTask, groupsTask, staffsTask, articlesTask, tagsTask, videosTask, lotteriesTask);

            var model = new EntryEditMetaOptions
            {
                EntryGameItems = gamesTask.Result ?? [],
                EntryRoleItems = rolesTask.Result ?? [],
                EntryGroupItems = groupsTask.Result ?? [],
                EntryStaffItems = staffsTask.Result ?? [],
                ArticleItems = articlesTask.Result ?? [],
                TagItems = tagsTask.Result ?? [],
                VideoItems = videosTask.Result ?? [],
                LotteryItems = lotteriesTask.Result ?? []
            };

            return SdkResult<EntryEditMetaOptions>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取词条候选列表数据异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<EntryEditMetaOptions>.Fail("ENTRY_META_OPTIONS_EXCEPTION", "请求词条候选列表数据时发生异常");
        }
    }

    public async Task<SdkResult<long>> SubmitEditAsync(EntryEditRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.IsCreate)
            {
                var response = await PostAsJsonRawAsync("api/entries/EstablishEntry", request.Data, cancellationToken);
                var result = await ReadResponseAsync<Result>(response, cancellationToken);
                
                if (result?.Successful == true && long.TryParse(result.Error, out var newId))
                {
                    return SdkResult<long>.Ok(newId);
                }
                
                return SdkResult<long>.Fail("ENTRY_CREATE_FAILED", result?.Error ?? "创建词条失败");
            }
            else
            {
                // 对于已有词条的更新，必须拆分为8个分块分别调用接口，以此复用后端的各分块审核流
                var errors = new List<string>();
                
                var responses = await Task.WhenAll(
                    SubmitPartAsync("api/entries/EditMain", request.Data.Main, cancellationToken),
                    SubmitPartAsync("api/entries/EditAddInfor", request.Data.AddInfor, cancellationToken),
                    SubmitPartAsync("api/entries/EditMainPage", request.Data.MainPage, cancellationToken),
                    SubmitPartAsync("api/entries/EditImages", request.Data.Images, cancellationToken),
                    SubmitPartAsync("api/entries/EditRelevances", request.Data.Relevances, cancellationToken),
                    SubmitPartAsync("api/entries/EditTags", request.Data.Tags, cancellationToken),
                    SubmitPartAsync("api/entries/EditAudio", request.Data.Audio, cancellationToken),
                    SubmitPartAsync("api/entries/EditWebsite", request.Data.Website, cancellationToken)
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
                    return SdkResult<long>.Fail("ENTRY_EDIT_PARTIAL_FAILED", string.Join("；", errors));
                }
                
                return SdkResult<long>.Ok(request.Data.Id); // 此时 Data.Id 就是原词条的 Id
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "提交词条编辑数据异常。IsCreate={IsCreate}; BaseAddress={BaseAddress}", request.IsCreate, HttpClient.BaseAddress);
            return SdkResult<long>.Fail("ENTRY_SUBMIT_EXCEPTION", "提交词条数据时发生异常");
        }
    }

    public async Task<SdkResult<List<EditInformationModel>>> GetInformationFieldsAsync(EntryType type, CancellationToken cancellationToken = default)
    {
        try
        {
            var informations = await GetFromJsonAsync<List<EditInformationModel>>($"api/entries/GetEditInformationModelList?type={(int)type}", cancellationToken);
            return SdkResult<List<EditInformationModel>>.Ok(informations ?? []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取词条基础信息字段列表异常。Type={Type}; BaseAddress={BaseAddress}", type, HttpClient.BaseAddress);
            return SdkResult<List<EditInformationModel>>.Fail("ENTRY_INFORMATION_FIELDS_EXCEPTION", "请求词条基础信息字段列表时发生异常");
        }
    }

    private Task<Result?> SubmitPartAsync<T>(string path, T model, CancellationToken cancellationToken)
    {
        return PostAsJsonAsync<T, Result>(path, model, cancellationToken);
    }
}
