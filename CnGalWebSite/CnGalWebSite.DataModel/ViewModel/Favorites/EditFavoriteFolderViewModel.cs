using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class EditFavoriteFolderViewModel
    {
        public long Id { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "是否为默认收藏夹")]
        public bool IsDefault { get; set; }

        [Display(Name = "主图")]
        public string MainImage { get; set; }

        public string MainImagePath { get; set; }
    }
}
