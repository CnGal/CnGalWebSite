using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class Almanac
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        public List<AlmanacEntry> Entries { get; set; }

        public List<AlmanacArticle> Articles { get; set; }
    }

    public class AlmanacEntry
    {
        public long Id { get; set; }

        public int EntryId { get; set; }
        public Entry Entry { get; set; }

        public long AlmanacId { get; set; }
        public Almanac Almanac { get; set; }

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

    public class AlmanacArticle
    {
        public long Id { get; set; }

        public long ArticleId { get; set; }
        public Article Article { get; set; }

        public long AlmanacId { get; set; }
        public Almanac Almanac { get; set; }

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
