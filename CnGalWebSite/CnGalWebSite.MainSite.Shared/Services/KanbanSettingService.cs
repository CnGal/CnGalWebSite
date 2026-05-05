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
            var size = await _jsRuntime.InvokeAsync<WindowSize>("getWindowSize");

            if (size.Width < 1600)
            {
                _kanban.Size.Width = 220;
                _kanban.Size.Height = 220;
                _button.Position.Left = 140;
                _dialogBox.Position.Left = -200;
                _dialogBox.Position.Bottom = 410;
                _chat.Position.Left = -200;
                _chat.Position.Bottom = 410;
            }

            _kanban.Position.Left = size.Width - _kanban.Size.Width;
            _kanban.Position.Top = size.Height - _kanban.Size.Height;
        }
        catch
        {
            // SSR or JS not available — use defaults
        }
    }

    private async Task ClampPositionsAsync()
    {
        try
        {
            var size = await _jsRuntime.InvokeAsync<WindowSize>("getWindowSize");
            var percentX = 0.5;
            var percentY = 0.8;

            // Kanban position
            if (_kanban.Position.Left + _kanban.Size.Width * percentX < 0)
                _kanban.Position.Left = 0;

            if (_kanban.Position.Left + _kanban.Size.Width * (1 - percentX) > size.Width)
                _kanban.Position.Left = (int)(size.Width - _kanban.Size.Width);

            if (_kanban.Position.Top + _kanban.Size.Height * percentY < 0)
                _kanban.Position.Top = 0;

            if (_kanban.Position.Top + _kanban.Size.Height * (1 - percentY) > size.Height)
                _kanban.Position.Top = (int)(size.Height - _kanban.Size.Height);

            // Kanban size
            if (_kanban.Size.Width < 150) _kanban.Size.Width = 150;
            if (_kanban.Size.Height < 150) _kanban.Size.Height = 150;
            if (_kanban.Size.Width > 600) _kanban.Size.Width = 600;
            if (_kanban.Size.Height > 600) _kanban.Size.Height = 600;

            // Button group
            ClampRelative(_button.Position.Left, -_kanban.Size.Width, _kanban.Size.Width * 2, v => _button.Position.Left = v);
            ClampRelative(_button.Position.Bottom, -_kanban.Size.Height, _kanban.Size.Height * 2, v => _button.Position.Bottom = v);

            // Dialog box
            ClampRelative(_dialogBox.Position.Left, -_kanban.Size.Width, _kanban.Size.Width * 4, v => _dialogBox.Position.Left = v);
            ClampRelative(_dialogBox.Position.Bottom, -_kanban.Size.Height, _kanban.Size.Height * 4, v => _dialogBox.Position.Bottom = v);

            // Chat card
            ClampRelative(_chat.Position.Left, -_kanban.Size.Width, _kanban.Size.Width * 4, v => _chat.Position.Left = v);
            ClampRelative(_chat.Position.Bottom, -_kanban.Size.Height, _kanban.Size.Height * 4, v => _chat.Position.Bottom = v);
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
