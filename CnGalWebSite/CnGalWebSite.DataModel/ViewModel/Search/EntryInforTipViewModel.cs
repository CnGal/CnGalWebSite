using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Search
{
    public class EntryInforTipViewModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "类型")]
        public EntryType Type { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "主图")]
        public string MainImage { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "最后编辑时间")]
        public DateTime LastEditTime { get; set; }
        /// <summary>
        /// 仅游戏词条
        /// </summary>
        [Display(Name = "发行时间")]
        public DateTime? PublishTime { get; set; }
        [Display(Name = "阅读数")]
        public int ReaderCount { get; set; }
        [Display(Name = "评论数")]
        public int CommentCount { get; set; }

        public List<EntryInforTipAddInforModel> AddInfors { get; set; } = new List<EntryInforTipAddInforModel>() { };

        public List<AudioViewModel> Audio { get; set; } = new List<AudioViewModel>();
    }

    public class EntryInforTipAddInforModel
    {
        public string Modifier { get; set; }

        public List<StaffNameModel> Contents { get; set; } = new List<StaffNameModel>() { };
    }
}
