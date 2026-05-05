using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;
using Microsoft.JSInterop;

namespace CnGalWebSite.MainSite.Shared.Services;

public class KanbanSettingService : IKanbanSettingService
{
    private readonly ILocalStorageService _localStorageService;
    private readonly IJSRuntime _jsRuntime;

    public event Action? OnSettingChanged;

    private KanbanSettingModel _kanban = new();
    private ButtonSettingModel _button = new();
    private DialogBoxSettingModel _dialogBox = new();
    private ChatCardSettingModel _chat = new();

    public KanbanSettingModel Kanban => _kanban;
    public ButtonSettingModel Button => _button;
    public DialogBoxSettingModel DialogBox => _dialogBox;
    public ChatCardSettingModel Chat => _chat;

    private const string StorageKey = "kanban_live2d_setting";

    public KanbanSettingService(ILocalStorageService localStorageService, IJSRuntime jsRuntime)
    {
        _localStorageService = localStorageService;
        _jsRuntime = jsRuntime;
    }

    public async Task LoadAsync()
    {
        var stored = await _localStorageService.GetItemAsync<SettingStorageModel>(StorageKey);
        if (stored is not null)
        {
            _kanban = stored.Kanban ?? new KanbanSettingModel();
            _button = stored.Button ?? new ButtonSettingModel();
            _dialogBox = stored.DialogBox ?? new DialogBoxSettingModel();
            _chat = stored.Chat ?? new ChatCardSettingModel();
        }
        else
        {
            await ResetToDefaultsAsync();
        }

        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        await ClampPositionsAsync();

        var storage = new SettingStorageModel
        {
            Kanban = _kanban,
            Button = _button,
            DialogBox = _dialogBox,
            Chat = _chat
        };

        await _localStorageService.SetItemAsync(StorageKey, storage);
        OnSettingChanged?.Invoke();
    }

    public async Task ResetAsync()
    {
        await ResetToDefaultsAsync();
        await SaveAsync();
    }

    public string GetStyles()
    {
        return $"position:fixed;left:{_kanban.Position.Left}px;top:{_kanban.Position.Top}px;";
    }

    private async Task ResetToDefaultsAsync()
    {
        _kanban = new KanbanSettingModel();
        _button = new ButtonSettingModel();
        _dialogBox = new DialogBoxSettingModel();
        _chat = new ChatCardSettingModel();

        try
        {
            var size = await GetWindowSizeAsync();
            if (size.Width < 768)
            {
                _kanban.ApplyMobileDefaults();
            }
        }
        catch
        {
            // SSR or JS not available — use desktop defaults
        }
    }

    private async Task ClampPositionsAsync()
    {
        try
        {
            var size = await GetWindowSizeAsync();

            // Kanban 尺寸：最小 150px，宽高分别最大为对应屏幕方向的 90%
            var maxWidth = (int)(size.Width * 0.9);
            var maxHeight = (int)(size.Height * 0.9);
            if (_kanban.Size.Width < 150) _kanban.Size.Width = 150;
            if (_kanban.Size.Height < 150) _kanban.Size.Height = 150;
            if (_kanban.Size.Width > maxWidth) _kanban.Size.Width = maxWidth;
            if (_kanban.Size.Height > maxHeight) _kanban.Size.Height = maxHeight;

            // 看板娘主体必须完整位于屏幕内，任意边被遮挡都推回可视区域。
            var kanbanWidth = _kanban.Size.Width;
            var kanbanHeight = _kanban.Size.Height;
            ClampRelative(_kanban.Position.Left, 0, Math.Max(0, size.Width - kanbanWidth), v => _kanban.Position.Left = v);
            ClampRelative(_kanban.Position.Top, 0, Math.Max(0, size.Height - kanbanHeight), v => _kanban.Position.Top = v);

            // Button group
            ClampRelative(_button.Position.Left, -kanbanWidth, kanbanWidth * 2, v => _button.Position.Left = v);
            ClampRelative(_button.Position.Bottom, -kanbanHeight, kanbanHeight * 2, v => _button.Position.Bottom = v);

            // Dialog box
            ClampRelative(_dialogBox.Position.Left, -kanbanWidth, kanbanWidth * 4, v => _dialogBox.Position.Left = v);
            ClampRelative(_dialogBox.Position.Bottom, -kanbanHeight, kanbanHeight * 4, v => _dialogBox.Position.Bottom = v);

            // Chat card
            ClampRelative(_chat.Position.Left, -kanbanWidth, kanbanWidth * 4, v => _chat.Position.Left = v);
            ClampRelative(_chat.Position.Bottom, -kanbanHeight, kanbanHeight * 4, v => _chat.Position.Bottom = v);
        }
        catch
        {
            // SSR or JS not available — skip clamping
        }
    }

    private static void ClampRelative(int value, int min, int max, Action<int> setter)
    {
        if (value < min)
            setter(min);
        else if (value > max)
            setter(max);
    }

    private async Task<WindowSize> GetWindowSizeAsync()
    {
        return await _jsRuntime.InvokeAsync<WindowSize>("eval", "({ height: window.innerHeight, width: document.documentElement.clientWidth || window.innerWidth })");
    }

    private sealed class SettingStorageModel
    {
        public KanbanSettingModel Kanban { get; set; } = new();
        public ButtonSettingModel Button { get; set; } = new();
        public DialogBoxSettingModel DialogBox { get; set; } = new();
        public ChatCardSettingModel Chat { get; set; } = new();
    }

    private sealed class WindowSize
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
