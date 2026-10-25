using CnGalWebSite.Extensions;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.SDK.MainSite.Models.ArticleEdit;

namespace CnGalWebSite.MainSite.Shared.Services.Editing;

public sealed class ArticleEditSession(IArticleCommandService ArticleCommandService,
    NavigationManager NavigationManager,
    ILocalStorageService LocalStorage,
    ICgToastService ToastService,
    AuthenticationStateProvider AuthenticationStateProvider) : IEditSession
{

    public long? Id { get; set; }

    public bool IsCreate => Id is null or <= 0;

    public bool IsLoading => Draft.IsLoading;
    public string? LoadErrorMessage { get; private set; }

    public bool IsSubmitting => Draft.IsSubmitting;

    public ArticleEditViewModel? Model { get; private set; }

    private string DraftKey => IsCreate ? "ArticleDraft_Create" : $"ArticleDraft_Edit_{Id}";

    public bool HasDraftRecovered => Draft.HasDraftRecovered;
    public bool IsAdmin { get; private set; }

    private EditDraftController<CreateArticleViewModel>? _draft;
    public EditDraftController<CreateArticleViewModel> Draft => _draft ??= new(
        DraftKey, LocalStorage, ToastService, () => Model?.Data, data => Model!.Data = data);
    public event Action? Changed { add => Draft.Changed += value; remove => Draft.Changed -= value; }
    public Task InitializeAsync() => Draft.InitializeAsync(InitializeDomainAsync);
    public Task LoadDataAsync() => Draft.LoadAsync(LoadCoreAsync);

    private async Task InitializeDomainAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        IsAdmin = authState.User.IsInRole("Admin");
        await LoadCoreAsync();
    }

    private async Task LoadCoreAsync()
    {
        LoadErrorMessage = null;

        try
        {
            SdkResult<ArticleEditViewModel> result;
            if (IsCreate)
            {
                result = await ArticleCommandService.GetArticleCreateTemplateAsync(Draft.Token);
            }
            else
            {
                result = await ArticleCommandService.GetArticleEditAsync(Id!.Value, Draft.Token);
            }

            if (result.Success && result.Data != null)
            {
                Model = result.Data;
                Model.Data = await Draft.RestoreAsync(result.Data.Data);

                // 获取关联候选列表
                var metaResult = await ArticleCommandService.GetArticleEditMetaOptionsAsync(Draft.Token);
                if (metaResult.Success && metaResult.Data != null)
                {
                    Model.MetaOptions = metaResult.Data;
                }
            }
            else
            {
                LoadErrorMessage = result.Error?.Message ?? "获取编辑数据失败";
            }
        }
        catch (OperationCanceledException) when (Draft.Token.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            LoadErrorMessage = "发生未知错误：" + ex.Message;
        }
    }

    public Task DiscardDraft() => Draft.DiscardAsync();

    private async Task SubmitCoreAsync()
    {
        if (Model == null) return;

        var validationResult = Model.Data.Validate();
        if (!validationResult.Successful)
        {
            ToastService.Show("校验失败", validationResult.Error ?? "请检查表单是否填写正确", "error");
            return;
        }

        try
        {
            var request = new ArticleEditRequest
            {
                IsCreate = IsCreate,
                Data = Model.Data
            };

            var result = await ArticleCommandService.SubmitEditAsync(request, Draft.Token);
            if (Draft.IsDisposed) return;
            if (result.Success)
            {
                await Draft.CompleteAsync();
                NavigationManager.NavigateTo($"/articles/index/{result.Data}");
            }
            else
            {
                ToastService.Show("提交失败", result.Error?.Message ?? "提交审核时出现问题", "error");
            }
        }
        catch (OperationCanceledException) when (Draft.Token.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            ToastService.Show("发生错误", ex.Message, "error");
        }
    }

    public Task Cancel() => Draft.SaveAndLeaveAsync(() =>
    {
        if (IsCreate)
        {
            NavigationManager.NavigateTo("/");
        }
        else
        {
            NavigationManager.NavigateTo($"/articles/index/{Id}");
        }
    });

    public Task SubmitAsync() => Draft.SubmitAsync(SubmitCoreAsync);
    public ValueTask DisposeAsync() => Draft.DisposeAsync();
}
