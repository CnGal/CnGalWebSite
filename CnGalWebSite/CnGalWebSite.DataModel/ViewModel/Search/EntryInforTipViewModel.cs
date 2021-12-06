using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Search
{
    public class EntryInforTipViewModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "类型")]
        public EntryType? Type { get; set; } = null;
        [Display(Name = "唯一名称")]
        public string Name { get; set; }
        [Display(Name = "显示名称")]
        public string DisplayName { get; set; }
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

    public class EntryInforTipAddInforModel
    {
        public string Modifier { get; set; }

        public List<string> Contents { get; set; } = new List<string>() { };
    }
}
