using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Votes
{
    public class VoteViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }
        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundPicture { get; set; }
        /// <summary>
        /// 小背景图
        /// </summary>
        public string SmallBackgroundPicture { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }
        /// <summary>
        /// 主页
        /// </summary>
        public string MainPage { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public VoteType Type { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        ///截止时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否允许修改
        /// </summary>
        public bool IsAllowModification { get; set; }

        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool CanComment { get; set; } = true;

        /// <summary>
        /// 参与投票的人数
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// 关联词条
        /// </summary>
        public List<EntryInforTipViewModel> Entries { get; set; } = new List<EntryInforTipViewModel>();
        /// <summary>
        /// 关联文章
        /// </summary>
        public List<ArticleInforTipViewModel> Artciles { get; set; } = new List<ArticleInforTipViewModel>();
        /// <summary>
        /// 关联周边
        /// </summary>
        public List<PeripheryInforTipViewModel> Peripheries { get; set; } = new List<PeripheryInforTipViewModel>();
        /// <summary>
        /// 选项
        /// </summary>
        public List<VoteOptionViewModel> Options { get; set; } = new List<VoteOptionViewModel>();
        /// <summary>
        /// 参加的用户
        /// </summary>
        public List<VoteUserViewModel> Users { get; set; } = new List<VoteUserViewModel>();

        public List<long> UserSelections { get; set; } = new List<long>();

    }

    public class VoteOptionViewModel
    {
        public long OptionId { get; set; }

        public long ObjectId { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public VoteOptionType Type { get; set; }

        /// <summary>
        /// 选择该选项的人数
        /// </summary>
        public long Count { get; set; }
    }

    public class VoteUserViewModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public DateTime VotedTime { get; set; }

        public string Image { get; set; }

        public string PersonalSignature { get; set; }

        public List<RankViewModel> Ranks { get; set; }
    }
}
