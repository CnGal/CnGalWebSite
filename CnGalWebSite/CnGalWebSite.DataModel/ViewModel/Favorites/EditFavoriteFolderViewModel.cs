using CnGalWebSite.DataModel.ViewModel.Base;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class EditFavoriteFolderViewModel:BaseEditModel
    {
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "默认收藏夹")]
        public bool IsDefault { get; set; }

        [Display(Name = "主图")]
        public string MainImage { get; set; }

        /// <summary>
        /// 是否向他人公开
        /// </summary>
        [Display(Name = "公开")]
        public bool ShowPublicly { get; set; }

        [Display(Name = "不展示在个人主页中")]
        public bool IsHidden { get; set; }
    }
}
