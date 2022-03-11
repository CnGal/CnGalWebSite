using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class EditArticleMainViewModel : BaseEditModel
    {
        [Display(Name = "显示名称")]
        [Required(ErrorMessage = "请填写显示名称")]
        public string DisplayName { get; set; }
        [Display(Name = "简介")]
        [Required(ErrorMessage = "请填写简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "主图")]
        public string MainPicture { get; set; }

        [Display(Name = "背景图")]
        public string BackgroundPicture { get; set; }

        [Display(Name = "小背景图")]
        public string SmallBackgroundPicture { get; set; }

        [Display(Name = "类别")]
        [Required(ErrorMessage = "请选择类别")]
        public ArticleType Type { get; set; }
        [Display(Name = "动态类别")]
        public string NewsType { get; set; }


        [Display(Name = "原作者")]
        public string OriginalAuthor { get; set; }
        [Display(Name = "原文链接")]
        public string OriginalLink { get; set; }
        [Display(Name = "发布日期")]
        public DateTime PubishTime { get; set; }
        [Display(Name = "动态发生时间")]
        public DateTime? RealNewsTime { get; set; }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name) )
            {
                return new Result { Error = "请填写唯一名称" };
            }
            if ( string.IsNullOrWhiteSpace(BriefIntroduction) )
            {
                return new Result { Error = "请填写简介" };
            }
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                return new Result { Error = "请填写显示名称" };
            }

            return new Result { Successful = true };
        }


    }
}
