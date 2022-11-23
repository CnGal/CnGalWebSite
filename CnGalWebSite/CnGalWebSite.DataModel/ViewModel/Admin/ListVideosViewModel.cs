using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{

    public class ListVideosViewModel
    {
        public List<ListVideoAloneModel> Videos { get; set; }
    }
    public class ListVideoAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "类型")]
        public string Type { get; set; }
        [Display(Name = "唯一名称")]
        public string Name { get; set; }
        [Display(Name = "显示名称")]
        public string DisplayName { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }
        [Display(Name = "最后编辑时间")]
        public DateTime LastEditTime { get; set; }
        [Display(Name = "阅读数")]
        public int ReaderCount { get; set; }
        [Display(Name = "原文作者")]
        public string OriginalAuthor { get; set; }
        [Display(Name = "原文发布时间")]
        public DateTime? PubishTime { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
        [Display(Name = "可否评论")]
        public bool CanComment { get; set; }
    }

    public class VideosPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListVideoAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
