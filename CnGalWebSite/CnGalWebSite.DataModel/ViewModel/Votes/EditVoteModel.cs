using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Votes
{
    public class EditVoteModel
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

        [Display(Name = "背景图")]
        public string BackgroundPicture { get; set; }

        [Display(Name = "小背景图")]
        public string SmallBackgroundPicture { get; set; }

        [Display(Name = "缩略图")]
        public string Thumbnail { get; set; }

        [Display(Name = "主页")]
        [StringLength(10000000)]
        public string MainPage { get; set; }

        [Display(Name = "类型")]
        public VoteType Type { get; set; }

        [Display(Name = "开始时间")]
        public DateTime BeginTime { get; set; }

        [Display(Name = "截止时间")]
        public DateTime EndTime { get; set; }

        [Display(Name = "是否允许修改")]
        public bool IsAllowModification { get; set; }
        [Display(Name = "同时选中项下限")]
        public long MinimumSelectionCount { get; set; }
        [Display(Name = "同时选中项上限")]
        public long MaximumSelectionCount { get; set; }

        [Display(Name = "备注")]
        public string Note { get; set; }

        public List<RelevancesModel> Roles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Staffs { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Groups { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Games { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Peripheries { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Articles { get; set; } = new List<RelevancesModel>();
        public List<EditVoteOptionModel> Options { get; set; } = new List<EditVoteOptionModel>();
    }
}
