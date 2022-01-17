using CnGalWebSite.DataModel.Model;
using System;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Search
{
    public class ArticleInforTipViewModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "类型")]
        public ArticleType? Type { get; set; } = null;
        [Display(Name = "唯一名称")]
        public string Name { get; set; }
        [Display(Name = "显示名称")]
        public string DisplayName { get; set; }
        [Display(Name = "创建文章的用户")]
        public string CreateUserName { get; set; }
        [Display(Name = "主图")]
        public string MainImage { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "最后编辑时间")]
        public DateTime LastEditTime { get; set; }
        [Display(Name = "阅读数")]
        public int ReaderCount { get; set; }
        [Display(Name = "点赞数")]
        public int ThumbsUpCount { get; set; }
        [Display(Name = "评论数")]
        public int CommentCount { get; set; }
        [Display(Name = "原文链接")]
        public string Link { get; set; }
    }
}
