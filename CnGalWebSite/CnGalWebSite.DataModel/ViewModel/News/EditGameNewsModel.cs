using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.News
{
    public class EditGameNewsModel
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
        [Display(Name = "动态类型")]
        public string NewsType { get; set; }
        [Display(Name = "原文发布时间")]
        public DateTime PublishTime { get; set; }


        [Display(Name = "作者")]
        public string Author { get; set; }
        [Display(Name = "微博Id")]
        public string WeiboId { get; set; }
        [Display(Name = "关联词条名称")]
        public string AuthorEntryName { get; set; }

        [Display(Name = "原文链接")]
        public string Link { get; set; }
        [Display(Name = "状态")]
        public GameNewsState State { get; set; }

        [Display(Name = "关联词条")]
        public List<RelevancesModel> Entries { get; set; } = new List<RelevancesModel>();

        public long? ArticleId { get; set; }
    }
}
