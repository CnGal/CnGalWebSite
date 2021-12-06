using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.Shared.AppComponent.Pages.Entries.Normal.Relevances
{
    public class StaffOrRoleCardModel
    {
        public string Image { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int EntryId { get; set; }

        public EntryInforTipAddInforModel InforModel { get; set; }
    }
}
