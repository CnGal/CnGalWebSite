using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListWeeklyNewsInforViewModel
    {
        public long Hiddens { get; set; }
        public long All { get; set; }
    }

    public class ListWeeklyNewsViewModel
    {
        public List<ListWeeklyNewAloneModel> WeeklyNews { get; set; }
}
    public class ListWeeklyNewAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "状态")]
        public GameNewsState State { get; set; }

        [Display(Name = "标题")]
        public string Title { get; set; }

        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "发布时间")]
        public DateTime PublishTime { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        [Display(Name = "文章Id")]
        public long ArticleId { get; set; }

    }

    public class WeeklyNewsPagesInfor
    {
        public QueryPageOptions Options { get; set; }
        public ListWeeklyNewAloneModel SearchModel { get; set; }
    }
}
