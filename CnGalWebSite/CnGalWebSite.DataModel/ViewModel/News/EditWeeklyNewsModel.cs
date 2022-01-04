using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.News
{
    public class EditWeeklyNewsModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "标题")]
        public string Title { get; set; }
        [Display(Name = "主图")]
        public string MainPicture { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "主页")]
        [StringLength(10000000)]
        public string MainPage { get; set; }
        [Display(Name = "类型")]
        public ArticleType Type { get; set; }

        public List<WeeklyNewsRelatedNewsEditModel> News { get; set; }=new List<WeeklyNewsRelatedNewsEditModel> { };
        [Display(Name = "发布时间")]
        public DateTime PublishTime { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }
        [Display(Name = "状态")]
        public GameNewsState State { get; set; }

        public long? ArticleId { get; set; }
    }

    public class WeeklyNewsRelatedNewsEditModel
    {
        public long NewsId { get; set; }

        public string NewsTitle { get; set; }

        public bool IsSelected { get; set; }
    }
}
