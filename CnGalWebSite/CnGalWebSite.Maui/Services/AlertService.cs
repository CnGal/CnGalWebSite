namespace CnGalWebSite.Maui.Services
{
    public class AlertService : IAlertService
    {

        private MainPage _page { get; set; }

        public void Init(MainPage page)
        {
            _page = page;
        }

        public async void ShowAlert(string Title, string Text)
        {
            await _page.DisplayAlert(Title, Text, "确定");
        }

    }
}
