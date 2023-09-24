﻿using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class LotteryViewModel
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
        public LotteryType Type { get; set; }

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
        ///抽奖时间
        /// </summary>
        public DateTime LotteryTime { get; set; }
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
        /// 是否可以评论
        /// </summary>
        public bool CanComment { get; set; } = true;

        /// <summary>
        /// 参与抽奖的人数
        /// </summary>
        public long Count { get; set; }

        public bool IsEnd { get; set; }

        public LotteryConditionType ConditionType { get; set; }

        public List<LotteryAwardViewModel> Awards { get; set; } = new List<LotteryAwardViewModel>();
    }


    public class LotteryAwardViewModel
    {
        public long Id { get; set; }

        public int Priority { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }

        public LotteryAwardType Type { get; set; }
        /// <summary>
        /// 赞助商
        /// </summary>
        public string Sponsor { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 链接 用于展示对于的游戏或贩售地址
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// 附加积分 全类型生效
        /// </summary>
        public int Integral { get; set; }

        /// <summary>
        /// 中奖的用户
        /// </summary>
        public List<UserInforViewModel> Users { get; set; } = new List<UserInforViewModel>();

    }
    public class LotteryUserViewModel: UserInforViewModel
    {
        public DateTime VotedTime { get; set; }
    }
}
