using CnGalWebSite.DataModel.ViewModel.Ranks;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class UserInforViewModel
    {
        public string Id { get; set; }
        public string BackgroundImage { get; set; }
        public string PhotoPath { get; set; }
        public string Name { get; set; }
        public string PersonalSignature { get; set; }
        public int Integral { get; set; }

        public int EditCount { get; set; }
        public int ArticleCount { get; set; }
        public int VideoCount { get; set; }
        public int ArticleReadCount { get; set; }

        public int SignInDays { get; set; }
        public bool IsSignIn { get; set; }

        public List<RankViewModel> Ranks { get; set; } = new List<RankViewModel>();

    }
}
