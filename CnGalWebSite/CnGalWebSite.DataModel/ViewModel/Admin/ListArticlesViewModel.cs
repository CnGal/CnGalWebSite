
using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class GetArticleCountModel
    {
        public long Toughts { get; set; }

        public long Strategies { get; set; }

        public long Interviews { get; set; }
        public long News { get; set; }
        public long Notices { get; set; }
        public long Others { get; set; }

        public long Hiddens { get; set; }
        public long All { get; set; }
    }

    public class ListArticlesViewModel
    {
        public List<ListArticleAloneModel> Articles { get; set; }
    }
    public class ListArticleAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "类型")]
        public ArticleType? Type { get; set; } = null;
        [Display(Name = "唯一名称")]
        public string Name { get; set; }
        [Display(Name = "显示名称")]
        public string DisplayName { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }
        [Display(Name = "最后编辑时间")]
        public DateTime LastEditTime { get; set; }
        [Display(Name = "阅读数")]
        public int ReaderCount { get; set; }
        // [Display(Name = "点赞数")]
        //public int ThumbsUpCount { get; set; }
        [Display(Name = "原文作者")]
        public string OriginalAuthor { get; set; }
        [Display(Name = "原文链接")]
        public string OriginalLink { get; set; }
        [Display(Name = "原文发布时间")]
        public DateTime? PubishTime { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
        [Display(Name = "可否评论")]
        public bool CanComment { get; set; }
    }

    public class ArticlesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListArticleAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
