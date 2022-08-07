
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListRanksInforViewModel
    {
        public long Hiddens { get; set; }
        public long All { get; set; }
    }

    public class ListRanksViewModel
    {
        public List<ListRankAloneModel> Ranks { get; set; }
    }
    public class ListRankAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "显示文本")]
        public string Text { get; set; }
        [Display(Name = "类型")]
        public RankType Type { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }
        [Display(Name = "人数")]
        public int Count { get; set; }
        [Display(Name = "CSS")]
        public string CSS { get; set; }
        [Display(Name = "Styles")]
        public string Styles { get; set; }
        [Display(Name = "图片")]
        public string Image { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }
        [Display(Name = "最后编辑时间")]
        public DateTime LastEditTime { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; } = false;
    }

    public class RanksPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListRankAloneModel SearchModel { get; set; }
    }
}
