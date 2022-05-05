namespace CnGalWebSite.Maui.Services
{
    internal class OverviewService : IOverviewService
    {
        private MainPage _page { get; set; }

        public void Init(MainPage page)
        {
            _page = page;
        }

        public void HideLoadingOverview()
        {
            _page.HideOverviewGrid();
        }

    }
}
