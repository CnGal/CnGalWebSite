using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Search
{
    public class PeripheryInforTipViewModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "唯一名称")]
        public string Name { get; set; }
        [Display(Name = "主图")]
        public string MainImage { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "最后编辑时间")]
        public DateTime LastEditTime { get; set; }
        [Display(Name = "阅读数")]
        public int ReaderCount { get; set; }
        [Display(Name = "评论数")]
        public int CommentCount { get; set; }

        public List<EntryInforTipAddInforModel> AddInfors { get; set; } = new List<EntryInforTipAddInforModel>() { };
    }
}
