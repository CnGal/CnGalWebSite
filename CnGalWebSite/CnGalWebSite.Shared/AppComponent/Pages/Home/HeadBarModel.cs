using CnGalWebSite.DataModel.ViewModel.Space;

namespace CnGalWebSite.Shared.AppComponent.Pages.Home
{
    public class HeadBarModel
    {
        public string Image { get; set; } = "/_content/CnGalWebSite.Shared/images/apps/user.png";

        public UserUnReadedMessagesModel UnreadedMessages { get; set; } = new UserUnReadedMessagesModel();

    }
}
