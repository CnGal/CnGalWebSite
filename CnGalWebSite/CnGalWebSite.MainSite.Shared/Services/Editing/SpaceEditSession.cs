using CnGalWebSite.Extensions;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.SDK.MainSite.Models.SpaceEdit;

namespace CnGalWebSite.MainSite.Shared.Services.Editing;

public sealed class SpaceEditSession(ISpaceCommandService SpaceCommandService,
    NavigationManager NavigationManager,
    ILocalStorageService LocalStorage,
    ICgToastService ToastService) : IEditSession
{

    public bool IsLoading => Draft.IsLoading;
    public string? LoadErrorMessage { get; private set; }
    public bool IsSubmitting => Draft.IsSubmitting;

    public SpaceEditViewModel? Model { get; private set; }

    private const string DraftKey = "SpaceDraft_Edit";

    public bool HasDraftRecovered => Draft.HasDraftRecovered;
    private SpaceEditViewModel? OriginalData => Draft.OriginalData;

    private EditDraftController<SpaceEditViewModel>? _draft;
    public EditDraftController<SpaceEditViewModel> Draft => _draft ??= new(
        DraftKey, LocalStorage, ToastService, () => Model, data => Model = data);
    public event Action? Changed { add => Draft.Changed += value; remove => Draft.Changed -= value; }
    public Task InitializeAsync() => Draft.InitializeAsync(LoadCoreAsync);
    public Task LoadDataAsync() => Draft.LoadAsync(LoadCoreAsync);

    private async Task LoadCoreAsync()
    {
        LoadErrorMessage = null;

        try
        {
            var result = await SpaceCommandService.GetSpaceEditAsync(Draft.Token);

            if (result.Success && result.Data != null)
            {
                Model = result.Data;
                Model = await Draft.RestoreAsync(result.Data);
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

        // 若原始值与当前值都是 null 或空字符串，保持原始值不变，避免产生空的审核记录
        if (string.IsNullOrEmpty(Model.MainPage.MainPage) && string.IsNullOrEmpty(OriginalData?.MainPage.MainPage))
        {
            Model.MainPage.MainPage = OriginalData?.MainPage.MainPage;
        }

        // 客户端校验
        var userDataValidation = Model.UserData.Validate();
        if (!userDataValidation.Successful)
        {
            ToastService.Show("校验失败", userDataValidation.Error ?? "请检查个人资料是否填写正确", "error");
            return;
        }

        var mainPageValidation = Model.MainPage.Validate();
        if (!mainPageValidation.Successful)
        {
            ToastService.Show("校验失败", mainPageValidation.Error ?? "请检查个人主页内容", "error");
            return;
        }

        try
        {
            var result = await SpaceCommandService.SubmitEditAsync(Model, Draft.Token);
            if (Draft.IsDisposed) return;
            if (result.Success)
            {
                await Draft.CompleteAsync();
                ToastService.Show("保存成功", "修改已提交，部分内容需要审核通过后才会对其他用户可见", "success");
                NavigationManager.NavigateTo($"/space/index/{result.Data}");
            }
            else
            {
                ToastService.Show("提交失败", result.Error?.Message ?? "提交时出现问题", "error");
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
        if (Model?.UserData?.Id != null)
        {
            NavigationManager.NavigateTo($"/space/index/{Model.UserData.Id}");
        }
        else
        {
            NavigationManager.NavigateTo("/");
        }
    });

    public Task SubmitAsync() => Draft.SubmitAsync(SubmitCoreAsync);
    public ValueTask DisposeAsync() => Draft.DisposeAsync();
}
