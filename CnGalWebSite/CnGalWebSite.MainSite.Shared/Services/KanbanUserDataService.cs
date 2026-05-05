using Blazored.LocalStorage;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;

namespace CnGalWebSite.MainSite.Shared.Services;

public class KanbanUserDataService : IKanbanUserDataService
{
    private readonly ILocalStorageService _localStorageService;

    private KanbanUserDataModel _userData = new();

    public KanbanUserDataModel UserData => _userData;

    private const string StorageKey = "kanban_userdata";

    public KanbanUserDataService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async Task LoadAsync()
    {
        var stored = await _localStorageService.GetItemAsync<KanbanUserDataModel>(StorageKey);
        if (stored is not null)
        {
            _userData = stored;
        }
        else
        {
            await ResetAsync();
        }
    }

    public async Task SaveAsync()
    {
        await _localStorageService.SetItemAsync(StorageKey, _userData);
    }

    public async Task ResetAsync()
    {
        _userData = new KanbanUserDataModel();
        await SaveAsync();
    }
}
