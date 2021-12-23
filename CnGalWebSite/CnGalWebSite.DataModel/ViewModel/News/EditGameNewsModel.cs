using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.News
{
    public class EditGameNewsModel
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

        public string NewsType { get; set; }

        public DateTime PublishTime { get; set; }



        public string Author { get; set; }

        public long WeiboId { get; set; }

        public string AuthorEntryName { get; set; }


        public string Link { get; set; }

        public GameNewsState State { get; set; }

        /// <summary>
        /// 关联词条
        /// </summary>
        public List<string> Entries { get; set; } = new List<string>();
    }
}
