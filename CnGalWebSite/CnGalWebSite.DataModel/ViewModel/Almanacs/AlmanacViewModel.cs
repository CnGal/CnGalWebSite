using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Almanacs
{
    public class AlmanacViewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = "CNGAL年鉴";
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; } = 2023;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        public List<AlmanacEntryViewModel> Entries { get; set; } = new List<AlmanacEntryViewModel>();

        public List<AlmanacArticleViewModel> Articles { get; set; } = new List<AlmanacArticleViewModel>();

    }

    public class AlmanacEntryViewModel
    {
        public long Id { get; set; }

        public EntryIndexViewModel Entry { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 是否已发布
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 游戏发布时间 只对游戏词条有效
        /// </summary>
        public DateTime? PublishTime { get; set; }

    }

    public class AlmanacArticleViewModel
    {
        public long Id { get; set; }

        public ArticleViewModel Article { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

    }
}
