using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Others
{
    public enum TrackEventType
    {
        [Display(Name = "浏览")]
        View,
        [Display(Name = "点击")]
        Click,
        [Display(Name = "登入")]
        Login,
    }

    public enum TrackEventDataType
    {
        [Display(Name = "词条")]
        Entry,
        [Display(Name = "文章")]
        Article,
        [Display(Name = "用户")]
        User,
        [Display(Name = "标签")]
        Tag,
        [Display(Name = "评论")]
        Comment,
        [Display(Name = "消歧义页")]
        Disambig,
        [Display(Name = "周边")]
        Periphery,
        [Display(Name = "游玩记录")]
        PlayedGame,
        [Display(Name = "用户认证")]
        UserCertification,
        [Display(Name = "视频")]
        Video,
        [Display(Name = "收藏夹")]
        FavoriteFolder,
        [Display(Name = "动态")]
        News,
        [Display(Name = "轮播图")]
        Carousel,
        [Display(Name = "抽奖")]
        Lottery,
        [Display(Name = "投票")]
        Vote,
    }
}
