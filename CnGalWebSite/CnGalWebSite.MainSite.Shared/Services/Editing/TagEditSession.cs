using CnGalWebSite.Extensions;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.SDK.MainSite.Models.TagEdit;

namespace CnGalWebSite.MainSite.Shared.Services.Editing;

public sealed class TagEditSession(ITagCommandService TagCommandService,
    ITagQueryService TagQueryService,
    NavigationManager NavigationManager,
    ILocalStorageService LocalStorage,
    ICgToastService ToastService) : IEditSession
{

    public int? Id { get; set; }

    public bool IsCreate => Id is null || Id <= 0;

    public bool IsLoading => Draft.IsLoading;
    public string? LoadErrorMessage { get; private set; }

    public bool IsSubmitting => Draft.IsSubmitting;

    public TagEditViewModel? Model { get; private set; }

    private string DraftKey => IsCreate ? "TagDraft_Create" : $"TagDraft_Edit_{Id}";

    public bool HasDraftRecovered => Draft.HasDraftRecovered;

    private EditDraftController<CreateTagViewModel>? _draft;
    public EditDraftController<CreateTagViewModel> Draft => _draft ??= new(
        DraftKey, LocalStorage, ToastService, () => Model?.Data, data => Model!.Data = data);
    public event Action? Changed { add => Draft.Changed += value; remove => Draft.Changed -= value; }
    public Task InitializeAsync() => Draft.InitializeAsync(LoadCoreAsync);
    public Task LoadDataAsync() => Draft.LoadAsync(LoadCoreAsync);

    private async Task LoadCoreAsync()
    {
        LoadErrorMessage = null;

        try
        {
            SdkResult<TagEditViewModel> result;
            if (IsCreate)
            {
                result = await TagCommandService.GetTagCreateTemplateAsync(Draft.Token);
            }
            else
            {
                result = await TagCommandService.GetTagEditAsync(Id!.Value, Draft.Token);
            }

            if (result.Success && result.Data != null)
            {
                Model = result.Data;
                Model.Data = await Draft.RestoreAsync(result.Data.Data);

                // 加载候选列表与标签树
                var metaTask = TagCommandService.GetTagEditMetaOptionsAsync(Draft.Token);
                var tagTreeTask = TagQueryService.GetTagTreeAsync(Draft.Token);
                await Task.WhenAll(metaTask, tagTreeTask);

                var metaResult = metaTask.Result;
                if (metaResult.Success && metaResult.Data != null)
                {
                    Model.MetaOptions = metaResult.Data;
                }

                var tagTreeResult = tagTreeTask.Result;
                if (tagTreeResult.Success && tagTreeResult.Data != null)
                {
                    Model.MetaOptions.TagTreeItems = tagTreeResult.Data;
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
            var request = new TagEditRequest
            {
                IsCreate = IsCreate,
                Data = Model.Data
            };

            var result = await TagCommandService.SubmitEditAsync(request, Draft.Token);
            if (Draft.IsDisposed) return;
            if (result.Success)
            {
                await Draft.CompleteAsync();
                NavigationManager.NavigateTo($"/tags/index/{result.Data}");
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
            NavigationManager.NavigateTo($"/tags/index/{Id}");
        }
    });

    public Task SubmitAsync() => Draft.SubmitAsync(SubmitCoreAsync);
    public ValueTask DisposeAsync() => Draft.DisposeAsync();
}
