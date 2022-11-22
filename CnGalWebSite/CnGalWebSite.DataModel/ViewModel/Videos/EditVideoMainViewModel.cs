using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Videos
{
    public class EditVideoMainViewModel : BaseEditModel
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
        public string Type { get; set; }

        [Display(Name = "版权")]
        public CopyrightType Copyright { get; set; }
        
        [Display(Name = "时长")]
        public TimeSpan Duration { get; set; }
       
        [Display(Name = "是否为互动视频")]
        public bool IsInteractive { get; set; }
     
        [Display(Name = "是否为用户本人创作")]
        public bool IsCreatedByCurrentUser { get; set; }

        [Display(Name = "原作者")]
        public string OriginalAuthor { get; set; }

        [Display(Name = "发布日期")]
        public DateTime PubishTime { get; set; }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Error = "请填写唯一名称" };
            }
            if (string.IsNullOrWhiteSpace(BriefIntroduction))
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
