
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{

    public class GameNewsOverviewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public GameNewsState State { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }

        /// <summary>
        /// 原文发布时间
        /// </summary>
        public DateTime PublishTime { get; set; }
        /// <summary>
        /// 文章Id
        /// </summary>
        public long ArticleId { get; set; }

    }
}
