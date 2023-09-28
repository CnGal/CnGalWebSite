using CnGalWebSite.DataModel.ViewModel.Ranks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public int GCoins { get; set; }

        public int EditCount { get; set; }
        public int ArticleCount { get; set; }
        public int VideoCount { get; set; }
        public int ArticleReadCount { get; set; }

        public int SignInDays { get; set; }
        public bool IsSignIn { get; set; }

        /// <summary>
        /// 用户背景图 大屏幕
        /// </summary>
        public string MBgImage { get; set; }

        /// <summary>
        /// 用户背景图 小屏幕
        /// </summary>
        public string SBgImage { get; set; }

        public List<RankViewModel> Ranks { get; set; } = new List<RankViewModel>();

    }

    public enum UserHeatMapType
    {
        [Display(Name ="编辑记录")]
        EditRecords,
        [Display(Name = "签到")]
        SignInDays
    }
}
