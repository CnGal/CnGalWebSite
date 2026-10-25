using Microsoft.AspNetCore.Components;
using System;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace CnGalWebSite.MainSite.Shared.Services;

public class KanbanLive2DService : IKanbanLive2DService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IConfiguration _configuration;
    private readonly IKanbanSettingService _settingService;
    private IJSObjectReference? _module;
    private IJSObjectReference? _controller;
    private ElementReference _owner;
    private long _generation;
    private DotNetObjectReference<KanbanLive2DService>? _objRef;
    private string? _kanbanImage;

    public KanbanSettingModel Kanban { get; } = new();
    public ButtonSettingModel Button { get; } = new();
    public DialogBoxSettingModel DialogBox { get; } = new();
    public ChatCardSettingModel Chat { get; } = new();
    public KanbanUserDataModel UserData { get; } = new();

    public event Action? Live2DInitialized;
    public event Action<string>? KanbanImageGenerated;
    public event Action<string>? CustomEventTriggered;
    public event Action<int>? MouseOverTriggered;

    public async Task ConfigureHoverAsync(IReadOnlyList<KanbanHoverTarget> targets)
    {
        if (_controller is not null && _objRef is not null)
            await _controller.InvokeVoidAsync("initMouseOverEvent", _objRef, targets);
    }

    [JSInvokable] public void TriggerMouseOverEventAsync(int index) => MouseOverTriggered?.Invoke(index);

    private const string ModulePath = "/_content/CnGalWebSite.MainSite.Shared/Components/Features/Kanban/KanbanRoot.razor.js";

    public KanbanLive2DService(IJSRuntime jsRuntime, IConfiguration configuration, IKanbanSettingService settingService)
    {
        _jsRuntime = jsRuntime;
        _configuration = configuration;
        _settingService = settingService;
    }

    public async Task InitAsync(string modelDir, int modelIndex, ElementReference root)
    {
        await ReleaseLive2D();
        var generation = ++_generation;
        _owner = root;
        var module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
        if (generation != _generation) { await module.DisposeAsync(); return; }
        IJSObjectReference controller;
        try { controller = await module.InvokeAsync<IJSObjectReference>("create", root); }
        catch { await module.DisposeAsync(); throw; }
        if (generation != _generation)
        {
            await controller.InvokeVoidAsync("dispose"); await controller.DisposeAsync(); await module.DisposeAsync(); return;
        }
        _module = module; _controller = controller;
        _objRef = DotNetObjectReference.Create(this);
        var resourcesPath = _configuration["Live2D_ResourcesPath"] ?? string.Empty;
        await controller.InvokeVoidAsync("initKanbanLive2D", _objRef, modelDir, modelIndex, resourcesPath);
    }

    public async Task InitResizeAsync()
    {
        if (_module is null || _objRef is null)
        {
            return;
        }

        await _controller!.InvokeVoidAsync("initKanbanResizeAction", _objRef);
    }

    public async Task SetModelAsync(int index)
    {
        if (_module is null)
        {
            return;
        }

        await _controller!.InvokeVoidAsync("switchLiv2DModel", index);
    }

    public async Task SetExpression(string name)
    {
        if (_module is null)
        {
            return;
        }

        await _controller!.InvokeVoidAsync("switchLiv2DExpression", name);
    }

    public async Task CleanExpression()
    {
        if (_module is null)
        {
            return;
        }

        await _controller!.InvokeVoidAsync("switchLiv2DExpression");
    }

    public async Task SetClothes(string name)
    {
        if (name is null)
        {
            return;
        }

        if (_module is null)
        {
            return;
        }

        UserData.ClothesName = name;

        if (name.EndsWith('0'))
        {
            await _controller!.InvokeVoidAsync("switchLiv2DClothes");
        }
        else
        {
            await _controller!.InvokeVoidAsync("switchLiv2DClothes", name);
        }
    }

    public async Task SetStockings(string name)
    {
        if (name is null)
        {
            return;
        }

        if (_module is null)
        {
            return;
        }

        UserData.StockingsName = name;

        if (name.EndsWith('0'))
        {
            await _controller!.InvokeVoidAsync("switchLiv2DStockings");
        }
        else
        {
            await _controller!.InvokeVoidAsync("switchLiv2DStockings", name);
        }
    }

    public async Task SetShoes(string name)
    {
        if (name is null)
        {
            return;
        }

        if (_module is null)
        {
            return;
        }

        UserData.ShoesName = name;

        if (name.EndsWith('0'))
        {
            await _controller!.InvokeVoidAsync("switchLiv2DShoes");
        }
        else
        {
            await _controller!.InvokeVoidAsync("switchLiv2DShoes", name);
        }
    }

    public async Task SetMotion(string group, int index)
    {
        if (_module is null)
        {
            return;
        }

        await _controller!.InvokeVoidAsync("switchLiv2DMotion", group, index);
    }

    public async Task CleanMotion()
    {
        if (_module is null)
        {
            return;
        }

        await _controller!.InvokeVoidAsync("switchLiv2DMotion", "Idle");
    }

    public async Task StartKanbanImageGeneration()
    {
        if (_module is null || _objRef is null)
        {
            return;
        }

        var (x, y, width, height) = Kanban.GetCircleKanbanImageSize();
        await _controller!.InvokeVoidAsync("startKanbanImageGeneration", _objRef, x, y, height, width);
    }

    public async Task<string?> GetKanbanImageGeneration()
    {
        _kanbanImage = null;
        await StartKanbanImageGeneration();

        for (var i = 0; i < 100; i++)
        {
            if (!string.IsNullOrWhiteSpace(_kanbanImage))
            {
                return _kanbanImage;
            }

            await Task.Delay(100);
        }

        return null;
    }

    public async Task CommitDialogBoxPositionAsync()
    {
        if (_module is null)
        {
            return;
        }

        var position = await _controller!.InvokeAsync<DialogBoxPositionResult?>("getCurrentDialogBoxPosition");
        if (position is null)
        {
            return;
        }

        await SetDialogBoxPosition(position.Left, position.Bottom, position.Top);
    }

    public async Task ReleaseLive2D(ElementReference? owner = null)
    {
        if (owner.HasValue && owner.Value.Id != _owner.Id) return;
        ++_generation;
        var controller = _controller; var module = _module;
        _controller = null; _module = null;
        try
        {
            if (controller is not null) { await controller.InvokeVoidAsync("dispose"); await controller.DisposeAsync(); }
            if (module is not null) await module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
        catch (TaskCanceledException) { }
        finally { _objRef?.Dispose(); _objRef = null; }
    }

    [JSInvokable]
    public void Live2dInitCallback()
    {
        Live2DInitialized?.Invoke();
    }

    [JSInvokable]
    public async Task SetKanbanPosition(int left, int top)
    {
        Kanban.Position.Left = left;
        Kanban.Position.Top = top;

        _settingService.Kanban.Position.Left = left;
        _settingService.Kanban.Position.Top = top;
        await _settingService.SaveAsync();
    }

    [JSInvokable]
    public async Task SetKanbanSize(int width, int height)
    {
        Kanban.Size.Width = width;
        Kanban.Size.Height = height;

        _settingService.Kanban.Size.Width = width;
        _settingService.Kanban.Size.Height = height;
        await _settingService.SaveAsync();
    }

    [JSInvokable]
    public void SetButtonGroupPosition(int left, int bottom)
    {
        Button.Position.Left = left;
        Button.Position.Bottom = bottom;
    }

    [JSInvokable]
    public async Task SetDialogBoxPosition(int left, int bottom, int top)
    {
        DialogBox.Position.Left = left;
        DialogBox.Position.Bottom = bottom;
        DialogBox.Position.Top = top;

        _settingService.DialogBox.Position.Left = left;
        _settingService.DialogBox.Position.Bottom = bottom;
        _settingService.DialogBox.Position.Top = top;
        await _settingService.SaveAsync();
    }

    [JSInvokable]
    public void SetChatCardPosition(int left, int bottom)
    {
        Chat.Position.Left = left;
        Chat.Position.Bottom = bottom;
    }

    [JSInvokable]
    public async Task OnKanbanImageGenerated(string url)
    {
        if (_kanbanImage is not null)
        {
            await _jsRuntime.InvokeVoidAsync("URL.revokeObjectURL", _kanbanImage);
        }

        _kanbanImage = url;
        KanbanImageGenerated?.Invoke(url);
    }

    [JSInvokable]
    public void CheckKanbanPositionAsync()
    {
    }

    [JSInvokable]
    public void TriggerCustomEventAsync(string eventName)
    {
        CustomEventTriggered?.Invoke(eventName);
    }

    public async ValueTask DisposeAsync() => await ReleaseLive2D();

    private sealed class DialogBoxPositionResult
    {
        public int Left { get; set; }

        public int Bottom { get; set; }

        public int Top { get; set; }
    }
}
