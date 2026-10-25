using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.SDK.MainSite.Abstractions;
using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.MainSite.Shared.Services.Editing;

/// <summary>One folder editing session; never shared across routes or circuits.</summary>
public sealed class FavoriteFolderEditSession(long id, IFavoriteFolderQueryService query,
    IFavoriteFolderCommandService command, NavigationManager navigation, ICgToastService toast) : IEditSession
{
    public bool IsCreateMode => id == 0;
    public bool IsLoading { get; private set; } = true;
    public bool IsSaving { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string Name { get; set; } = string.Empty;
    public string BriefIntroduction { get; set; } = string.Empty;
    public string MainImage { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool ShowPublicly { get; set; } = true;
    public bool IsHidden { get; set; }
    public event Action? Changed;

    private readonly CancellationTokenSource _lifetime = new();
    private Task _loading = Task.CompletedTask, _saving = Task.CompletedTask;
    private Task? _disposal;
    private bool _disposed, _submitted;

    public Task InitializeAsync() => _loading = LoadAsync();
    private async Task LoadAsync()
    {
        try
        {
            if (IsCreateMode) return;
            var result = await query.GetFavoriteFolderForEditAsync(id, _lifetime.Token);
            _lifetime.Token.ThrowIfCancellationRequested();
            if (!result.Success || result.Data is null)
            {
                ErrorMessage = result.Error?.Message ?? "获取编辑数据失败";
                return;
            }
            var data = result.Data;
            Name = data.Name ?? string.Empty;
            BriefIntroduction = data.BriefIntroduction ?? string.Empty;
            MainImage = data.MainImage ?? string.Empty;
            IsDefault = data.IsDefault;
            ShowPublicly = data.ShowPublicly;
            IsHidden = data.IsHidden;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        finally { IsLoading = false; }
    }

    public Task SubmitAsync() => IsSaving || _submitted || _disposed ? _saving : _saving = SaveAsync();
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            toast.Show("必须填写收藏夹名称", variant: "error");
            return;
        }
        IsSaving = true;
        Changed?.Invoke();
        try
        {
            var result = IsCreateMode
                ? await command.CreateFavoriteFolderAsync(new CreateFavoriteFolderViewModel
                {
                    Name = Name, BriefIntroduction = BriefIntroduction, MainImage = MainImage,
                    IsDefault = IsDefault, ShowPublicly = ShowPublicly, IsHidden = IsHidden
                }, _lifetime.Token)
                : await command.EditFavoriteFolderAsync(new EditFavoriteFolderViewModel
                {
                    Id = id, Name = Name, BriefIntroduction = BriefIntroduction, MainImage = MainImage,
                    IsDefault = IsDefault, ShowPublicly = ShowPublicly, IsHidden = IsHidden
                }, _lifetime.Token);
            _lifetime.Token.ThrowIfCancellationRequested();
            if (result.Success)
            {
                _submitted = true;
                toast.Show(IsCreateMode ? "创建收藏夹成功" : "保存收藏夹成功", variant: "success");
                Cancel();
            }
            else toast.Show(result.Error?.Message ?? "保存失败", variant: "error");
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        finally
        {
            IsSaving = false;
            if (!_disposed) Changed?.Invoke();
        }
    }

    public void Cancel()
    {
        if (!_disposed) navigation.NavigateTo(IsCreateMode ? "/space/index" : $"/favoritefolders/index/{id}");
    }

    public ValueTask DisposeAsync() => new(_disposal ??= DisposeCoreAsync());
    private async Task DisposeCoreAsync()
    {
        _disposed = true;
        await _lifetime.CancelAsync();
        try { await Task.WhenAll(_loading, _saving); }
        finally { _lifetime.Dispose(); }
    }
}
