namespace CnGalWebSite.Maui.Services
{
    public interface IAlertService
    {
        void ShowAlert(string Title, string Text);

        void Init(MainPage page);
    }
}
