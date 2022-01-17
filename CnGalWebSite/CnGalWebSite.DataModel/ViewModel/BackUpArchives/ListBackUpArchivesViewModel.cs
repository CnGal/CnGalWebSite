
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.BackUpArchives
{
    public class ListBackUpArchivesInforViewModel
    {
        public long IsLastFail { get; set; }

        public long All { get; set; }
    }

    public class ListBackUpArchivesViewModel
    {
        public List<ListBackUpArchiveAloneModel> BackUpArchives { get; set; }
    }
    public class ListBackUpArchiveAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "上次备份时间")]
        public DateTime LastBackUpTime { get; set; }
        [Display(Name = "上次备份用时")]
        public double LastTimeUsed { get; set; }

        [Display(Name = "关联词条Id")]
        public int? EntryId { get; set; }
        [Display(Name = "关联文章Id")]
        public long? ArticleId { get; set; }

        [Display(Name = "上次是否失败")]
        public bool IsLastFail { get; set; } = false;
    }

    public class BackUpArchivesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListBackUpArchiveAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
