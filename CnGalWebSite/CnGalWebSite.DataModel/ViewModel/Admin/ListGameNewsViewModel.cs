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
    public class ListGameNewsInforViewModel
    {
        public long GameNews { get; set; }
        public long PublishedNews { get; set; }
        public long DeletedNews { get; set; }
        public long WeelyNews { get; set; }
        public long All { get; set; }
    }

    public class ListGameNewsViewModel
    {
        public List<ListGameNewAloneModel> GameNews { get; set; }
    }
    public class ListGameNewAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "状态")]
        public GameNewsState State { get; set; }
        [Display(Name = "作者")]
        public string Author { get; set; }
        [Display(Name = "标题")]
        public string Title { get; set; }

        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "原文发布时间")]
        public DateTime PublishTime { get; set; }
        [Display(Name = "文章Id")]
        public long ArticleId { get; set; }

    }

    public class GameNewsPagesInfor
    {
        public QueryPageOptions Options { get; set; }
        public ListGameNewAloneModel SearchModel { get; set; }
    }
}
