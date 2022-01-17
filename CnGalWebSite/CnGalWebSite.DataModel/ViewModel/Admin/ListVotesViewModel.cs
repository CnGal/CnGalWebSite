using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListVotesInforViewModel
    {
        public long Hiddens { get; set; }

        public long All { get; set; }
    }

    public class ListVotesViewModel
    {
        public List<ListVoteAloneModel> Votes { get; set; } = new List<ListVoteAloneModel> { };
    }
    public class ListVoteAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "类型")]
        public VoteType Type { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "开始时间")]
        public DateTime BeginTime { get; set; }
        [Display(Name = "截止时间")]
        public DateTime EndTime { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }

        [Display(Name = "阅读数")]
        public int ReaderCount { get; set; }

        [Display(Name = "评论数")]
        public int CommentCount { get; set; }

        [Display(Name = "最后编辑时间")]
        public DateTime? LastEditTime { get; set; }

        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }

        [Display(Name = "是否可以评论")]
        public bool CanComment { get; set; }
    }

    public class VotesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListVoteAloneModel SearchModel { get; set; }
    }

}
