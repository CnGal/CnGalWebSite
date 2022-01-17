
using CnGalWebSite.DataModel.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListEntriesInforViewModel
    {
        public int Roles { get; set; }

        public int Games { get; set; }

        public int Staffs { get; set; }
        public int Groups { get; set; }
        public int Hiddens { get; set; }

        public int All { get; set; }

    }
    public class ListEntriesViewModel
    {
        public List<ListEntryAloneModel> Entries { get; set; }
    }
    public class ListEntryAloneModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "类型")]
        public EntryType? Type { get; set; } = null;
        [Display(Name = "唯一名称")]
        public string Name { get; set; }

        [Display(Name = "显示名称")]
        public string DisplayName { get; set; }

        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
        [Display(Name = "可否评论")]
        public bool CanComment { get; set; }
    }

    public class EntriesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListEntryAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
