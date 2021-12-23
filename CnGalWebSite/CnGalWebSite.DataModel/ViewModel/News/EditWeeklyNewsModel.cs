using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.News
{
    public class EditWeeklyNewsModel
    {
        public long Id { get; set; }

        public string Title { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 主页
        /// </summary>
        [StringLength(10000000)]
        public string MainPage { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public ArticleType Type { get; set; }

        public List<WeeklyNewsRelatedNewsEditModel> News { get; set; }=new List<WeeklyNewsRelatedNewsEditModel> { };

        public DateTime PublishTime { get; set; }
        public DateTime CreateTime { get; set; }

        public GameNewsState State { get; set; }
    }

    public class WeeklyNewsRelatedNewsEditModel
    {
        public long NewsId { get; set; }

        public string NewsTitle { get; set; }

        public bool IsSelected { get; set; }
    }
}
