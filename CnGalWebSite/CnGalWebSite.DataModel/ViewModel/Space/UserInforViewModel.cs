using CnGalWebSite.DataModel.ViewModel.Ranks;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class UserInforViewModel
    {
        public string Id { get; set; }
        public string BackgroundImage { get; set; }
        public string PhotoPath { get; set; } = "/_content/CnGalWebSite.Shared/images/apps/user.png";
        public string UserName { get; set; }
        public string PersonalSignature { get; set; }
        public int Integral { get; set; }

        public int EditCount { get; set; }
        public int ArticleCount { get; set; }
        public int FavoriteCount { get; set; }

        public List<RankViewModel> Ranks { get; set; }

    }
}
