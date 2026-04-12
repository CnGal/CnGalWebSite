using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.ArticleEdit;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class ArticleCommandService(
    HttpClient httpClient,
    ILogger<ArticleCommandService> logger) : CommandServiceBase(httpClient), IArticleCommandService
{
    protected override ILogger Logger => logger;

    public Task<SdkResult<ArticleEditViewModel>> GetArticleCreateTemplateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new ArticleEditViewModel
            {
                IsCreate = true,
                Data = new CreateArticleViewModel
                {
                    Main = new EditArticleMainViewModel
                    {
                        PubishTime = DateTime.Now
                    }
                }
            };

            return Task.FromResult(SdkResult<ArticleEditViewModel>.Ok(model));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取文章创建模板异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return Task.FromResult(SdkResult<ArticleEditViewModel>.Fail("ARTICLE_CREATE_TEMPLATE_EXCEPTION", "请求文章创建模板时发生异常"));
        }
    }

    public async Task<SdkResult<ArticleEditViewModel>> GetArticleEditAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var mainTask = GetFromJsonAsync<EditArticleMainViewModel>($"api/articles/EditMain/{id}", cancellationToken);
            var mainPageTask = GetFromJsonAsync<EditArticleMainPageViewModel>($"api/articles/EditMainPage/{id}", cancellationToken);
            var relevancesTask = GetFromJsonAsync<EditArticleRelevancesViewModel>($"api/articles/EditRelevances/{id}", cancellationToken);

            await Task.WhenAll(mainTask, mainPageTask, relevancesTask);

            var model = new ArticleEditViewModel
            {
                IsCreate = false,
                Data = new CreateArticleViewModel
                {
                    Main = mainTask.Result ?? new(),
                    MainPage = mainPageTask.Result ?? new(),
                    Relevances = relevancesTask.Result ?? new()
                }
            };

            // 设置 Id 和 Name 以便后续提交
            model.Data.Id = id;
            model.Data.Name = model.Data.Main.Name;
            model.Data.Main.Id = id;
            model.Data.MainPage.Id = id;
            model.Data.Relevances.Id = id;
            model.Data.MainPage.Name = model.Data.Main.Name;
            model.Data.Relevances.Name = model.Data.Main.Name;

            return SdkResult<ArticleEditViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取文章编辑数据异常。ArticleId={ArticleId}; BaseAddress={BaseAddress}", id, HttpClient.BaseAddress);
            return SdkResult<ArticleEditViewModel>.Fail("ARTICLE_EDIT_QUERY_EXCEPTION", "请求文章编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<ArticleEditMetaOptions>> GetArticleEditMetaOptionsAsync(CancellationToken cancellationToken = default)
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

            var model = new ArticleEditMetaOptions
            {
                EntryGameItems = gamesTask.Result ?? [],
                EntryRoleItems = rolesTask.Result ?? [],
                EntryGroupItems = groupsTask.Result ?? [],
                EntryStaffItems = staffsTask.Result ?? [],
                ArticleItems = articlesTask.Result ?? [],
                VideoItems = videosTask.Result ?? []
            };

            return SdkResult<ArticleEditMetaOptions>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取文章候选列表数据异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<ArticleEditMetaOptions>.Fail("ARTICLE_META_OPTIONS_EXCEPTION", "请求文章候选列表数据时发生异常");
        }
    }

    public async Task<SdkResult<long>> SubmitEditAsync(ArticleEditRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.IsCreate)
            {
                var response = await PostAsJsonRawAsync("api/articles/Create", request.Data, cancellationToken);
                var result = await ReadResponseAsync<Result>(response, cancellationToken);

                if (result?.Successful == true && long.TryParse(result.Error, out var newId))
                {
                    return SdkResult<long>.Ok(newId);
                }

                return SdkResult<long>.Fail("ARTICLE_CREATE_FAILED", result?.Error ?? "创建文章失败");
            }
            else
            {
                var errors = new List<string>();

                var responses = await Task.WhenAll(
                    SubmitPartAsync("api/articles/EditMain", request.Data.Main, cancellationToken),
                    SubmitPartAsync("api/articles/EditMainPage", request.Data.MainPage, cancellationToken),
                    SubmitPartAsync("api/articles/EditRelevances", request.Data.Relevances, cancellationToken)
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
                    return SdkResult<long>.Fail("ARTICLE_EDIT_PARTIAL_FAILED", string.Join("；", errors));
                }

                return SdkResult<long>.Ok(request.Data.Main.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "提交文章编辑数据异常。IsCreate={IsCreate}; BaseAddress={BaseAddress}", request.IsCreate, HttpClient.BaseAddress);
            return SdkResult<long>.Fail("ARTICLE_SUBMIT_EXCEPTION", "提交文章数据时发生异常");
        }
    }

    private Task<Result?> SubmitPartAsync<T>(string path, T model, CancellationToken cancellationToken)
    {
        return PostAsJsonAsync<T, Result>(path, model, cancellationToken);
    }
}
