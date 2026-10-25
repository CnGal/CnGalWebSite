using CnGalWebSite.Extensions;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.DataModel.ViewModel.Lotteries;

namespace CnGalWebSite.MainSite.Shared.Services.Editing;

public sealed class LotteryEditSession(ILotteryCommandService LotteryCommandService,
    NavigationManager NavigationManager,
    ILocalStorageService LocalStorage,
    ICgToastService ToastService) : IEditSession
{

    public long? Id { get; set; }

    public bool IsCreate => Id is null or <= 0;

    public bool IsLoading => Draft.IsLoading;
    public string? LoadErrorMessage { get; private set; }

    public bool IsSubmitting => Draft.IsSubmitting;

    public EditLotteryModel? Model { get; private set; }

    private string DraftKey => IsCreate ? "LotteryDraft_Create" : $"LotteryDraft_Edit_{Id}";

    public bool HasDraftRecovered => Draft.HasDraftRecovered;
    public IReadOnlyList<string> EntryGameItems { get; private set; } = [];

    private EditDraftController<EditLotteryModel>? _draft;
    public EditDraftController<EditLotteryModel> Draft => _draft ??= new(
        DraftKey, LocalStorage, ToastService, () => Model, data => Model = data);
    public event Action? Changed { add => Draft.Changed += value; remove => Draft.Changed -= value; }
    public Task InitializeAsync() => Draft.InitializeAsync(LoadCoreAsync);
    public Task LoadDataAsync() => Draft.LoadAsync(LoadCoreAsync);

    private async Task LoadCoreAsync()
    {
        LoadErrorMessage = null;

        try
        {
            // 获取游戏词条列表
            var itemsResult = await LotteryCommandService.GetEntryGameItemsAsync(Draft.Token);
            if (itemsResult.Success && itemsResult.Data != null)
            {
                EntryGameItems = itemsResult.Data;
            }

            if (IsCreate)
            {
                Model = new EditLotteryModel
                {
                    BeginTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    LotteryTime = DateTime.Now,
                };
            }
            else
            {
                var result = await LotteryCommandService.GetLotteryEditAsync(Id!.Value, Draft.Token);
                if (result.Success && result.Data != null)
                {
                    Model = result.Data;
                }
                else
                {
                    LoadErrorMessage = result.Error?.Message ?? "获取抽奖编辑数据失败";
                    return;
                }
            }

            Model = await Draft.RestoreAsync(Model);
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

        var validationResult = Model.Validate();
        if (!validationResult.Successful)
        {
            ToastService.Show("校验失败", validationResult.Error ?? "请检查表单是否填写正确", "error");
            return;
        }

        try
        {
            SdkResult<long> result;

            if (IsCreate)
            {
                result = await LotteryCommandService.CreateLotteryAsync(Model, Draft.Token);
            }
            else
            {
                result = await LotteryCommandService.EditLotteryAsync(Model, Draft.Token);
            }

            if (Draft.IsDisposed) return;
            if (result.Success)
            {
                await Draft.CompleteAsync();
                NavigationManager.NavigateTo($"/lotteries/index/{result.Data}");
            }
            else
            {
                ToastService.Show(IsCreate ? "创建失败" : "编辑失败", result.Error?.Message ?? "提交时出现问题", "error");
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
            NavigationManager.NavigateTo("/lotteries");
        }
        else
        {
            NavigationManager.NavigateTo($"/lotteries/index/{Id}");
        }
    });

    public Task SubmitAsync() => Draft.SubmitAsync(SubmitCoreAsync);
    public ValueTask DisposeAsync() => Draft.DisposeAsync();
}
