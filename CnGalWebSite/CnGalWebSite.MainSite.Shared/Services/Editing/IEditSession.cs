namespace CnGalWebSite.MainSite.Shared.Services.Editing;

public interface IEditSession : IAsyncDisposable
{
    event Action? Changed;
    Task InitializeAsync();
}
