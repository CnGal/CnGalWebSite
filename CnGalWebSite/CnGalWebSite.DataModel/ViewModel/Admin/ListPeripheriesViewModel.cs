
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListPeripheriesInforViewModel
    {
        public long Hiddens { get; set; }

        public long All { get; set; }
    }

    public class ListPeripheriesViewModel
    {
        public List<ListPeripheryAloneModel> Peripheries { get; set; } = new List<ListPeripheryAloneModel> { };
    }
    public class ListPeripheryAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "作者")]
        public string Author { get; set; }
        [Display(Name = "材质")]
        public string Material { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }

        [Display(Name = "阅读数")]
        public int ReaderCount { get; set; }
        [Display(Name = "收集数")]
        public int CollectedCount { get; set; }
        [Display(Name = "评论数")]
        public int CommentCount { get; set; }

        [Display(Name = "最后编辑时间")]
        public DateTime? LastEditTime { get; set; }

        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }

        [Display(Name = "是否可以评论")]
        public bool CanComment { get; set; }
    }

    public class PeripheriesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListPeripheryAloneModel SearchModel { get; set; }
    }
}
