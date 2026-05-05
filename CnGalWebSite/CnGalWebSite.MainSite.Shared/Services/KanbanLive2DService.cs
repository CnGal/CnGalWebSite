using System;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace CnGalWebSite.MainSite.Shared.Services;

public class KanbanLive2DService : IKanbanLive2DService, IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IConfiguration _configuration;
    private readonly IKanbanSettingService _settingService;
    private IJSObjectReference? _module;
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

    private const string ModulePath = "/_content/CnGalWebSite.MainSite.Shared/scripts/cg-kanban-live2d.js";

    public KanbanLive2DService(IJSRuntime jsRuntime, IConfiguration configuration, IKanbanSettingService settingService)
    {
        _jsRuntime = jsRuntime;
        _configuration = configuration;
        _settingService = settingService;
    }

    public async Task InitAsync(string modelDir, int modelIndex)
    {
        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
        _objRef = DotNetObjectReference.Create(this);

        var resourcesPath = _configuration["Live2D_ResourcesPath"] ?? string.Empty;
        await _module.InvokeVoidAsync("initKanbanLive2D", _objRef, modelDir, modelIndex, resourcesPath);
    }

    public async Task SetModelAsync(int index)
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("switchLiv2DModel", index);
    }

    public async Task SetExpression(string name)
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("switchLiv2DExpression", name);
    }

    public async Task CleanExpression()
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("switchLiv2DExpression");
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
            await _module.InvokeVoidAsync("switchLiv2DClothes");
        }
        else
        {
            await _module.InvokeVoidAsync("switchLiv2DClothes", name);
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
            await _module.InvokeVoidAsync("switchLiv2DStockings");
        }
        else
        {
            await _module.InvokeVoidAsync("switchLiv2DStockings", name);
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
            await _module.InvokeVoidAsync("switchLiv2DShoes");
        }
        else
        {
            await _module.InvokeVoidAsync("switchLiv2DShoes", name);
        }
    }

    public async Task SetMotion(string group, int index)
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("switchLiv2DMotion", group, index);
    }

    public async Task CleanMotion()
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("switchLiv2DMotion", "Idle");
    }

    public async Task StartKanbanImageGeneration()
    {
        if (_module is null || _objRef is null)
        {
            return;
        }

        var (x, y, width, height) = Kanban.GetCircleKanbanImageSize();
        await _module.InvokeVoidAsync("startKanbanImageGeneration", _objRef, x, y, height, width);
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

    public async Task ReleaseLive2D()
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("releaseLive2D");
    }

    [JSInvokable]
    public void Live2dInitCallback()
    {
        Live2DInitialized?.Invoke();
    }

    [JSInvokable]
    public void SetKanbanPosition(int left, int top)
    {
        Kanban.Position.Left = left;
        Kanban.Position.Top = top;

        _settingService.Kanban.Position.Left = left;
        _settingService.Kanban.Position.Top = top;
        _ = _settingService.SaveAsync();
    }

    [JSInvokable]
    public void SetButtonGroupPosition(int left, int bottom)
    {
        Button.Position.Left = left;
        Button.Position.Bottom = bottom;
    }

    [JSInvokable]
    public void SetDialogBoxPosition(int left, int bottom)
    {
        DialogBox.Position.Left = left;
        DialogBox.Position.Bottom = bottom;
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

    public void Dispose()
    {
        _objRef?.Dispose();
        GC.SuppressFinalize(this);
    }
}
