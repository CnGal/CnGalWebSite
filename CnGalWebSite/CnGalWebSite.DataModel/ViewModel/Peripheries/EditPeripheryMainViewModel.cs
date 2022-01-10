using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class EditPeripheryMainViewModel
    {
        public long Id { get; set; }

        [Display(Name = "唯一名称")]
        [Required(ErrorMessage = "请填写唯一名称")]
        public string Name { get; set; }
        [Display(Name = "显示名称")]
        [Required(ErrorMessage = "请填写显示名称")]
        public string DisplayName { get; set; }

        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "主图")]
        public string MainPicture { get; set; }

        [Display(Name = "缩略图")]
        public string Thumbnail { get; set; }

        [Display(Name = "背景图")]
        public string BackgroundPicture { get; set; }

        [Display(Name = "小背景图")]
        public string SmallBackgroundPicture { get; set; }

        [Display(Name = "分类")]
        public PeripheryType Type { get; set; }
        #region 通用属性
        [Display(Name = "尺寸")]
        public string Size { get; set; }
        [Display(Name = "类别")]
        public string Category { get; set; }
        [Display(Name = "品牌")]
        public string Brand { get; set; }
        [Display(Name = "作者")]
        public string Author { get; set; }
        [Display(Name = "材质")]
        public string Material { get; set; }
        [Display(Name = "单独部件数量")]
        public string IndividualParts { get; set; }
        [Display(Name = "贩售链接")]
        public string SaleLink { get; set; }

        [Display(Name = "价格")]
        public string Price { get; set; }
        [Display(Name = "是否为再版")]
        public bool IsReprint { get; set; }
        [Display(Name = "是否为装饰品")]
        public bool IsAvailableItem { get; set; }
        #endregion
        #region 设定集
        [Display(Name = "页数")]
        public int PageCount { get; set; }
        #endregion
        #region 设定集
        [Display(Name = "歌曲数")]
        public int SongCount { get; set; }
        #endregion

        [Display(Name = "备注")]
        public string Note { get; set; }
    }
}
