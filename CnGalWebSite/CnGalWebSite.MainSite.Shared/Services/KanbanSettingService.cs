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

    private Task ResetToDefaultsAsync()
    {
        _kanban = new KanbanSettingModel();
        _button = new ButtonSettingModel();
        _dialogBox = new DialogBoxSettingModel();
        _chat = new ChatCardSettingModel();

        return Task.CompletedTask;
    }

    private async Task ClampPositionsAsync()
    {
        try
        {
            var size = await _jsRuntime.InvokeAsync<WindowSize>("getWindowSize");

            // 确保至少 50% 看板娘在屏幕内可见
            var kanbanLeft = _kanban.Position.Left;
            var kanbanTop = _kanban.Position.Top;
            var kanbanWidth = _kanban.Size.Width;
            var kanbanHeight = _kanban.Size.Height;

            if (kanbanLeft + kanbanWidth * 0.5 < 0)
                _kanban.Position.Left = (int)(-kanbanWidth * 0.5);

            if (kanbanLeft + kanbanWidth * 0.5 > size.Width)
                _kanban.Position.Left = (int)(size.Width - kanbanWidth * 0.5);

            if (kanbanTop + kanbanHeight * 0.5 < 0)
                _kanban.Position.Top = (int)(-kanbanHeight * 0.5);

            if (kanbanTop + kanbanHeight * 0.5 > size.Height)
                _kanban.Position.Top = (int)(size.Height - kanbanHeight * 0.5);

            // Kanban 尺寸：最小 150px，最大为屏幕宽高的 80%
            var maxSize = (int)(Math.Min(size.Width, size.Height) * 0.8);
            if (_kanban.Size.Width < 150) _kanban.Size.Width = 150;
            if (_kanban.Size.Height < 150) _kanban.Size.Height = 150;
            if (_kanban.Size.Width > maxSize) _kanban.Size.Width = maxSize;
            if (_kanban.Size.Height > maxSize) _kanban.Size.Height = maxSize;

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
