using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.Model
{
    public class Lottery
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
        [StringLength(10000000)]
        public string MainPage { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public LotteryType Type { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

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
        /// 关联词条 预约活动
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }
        /// <summary>
        /// 是否抽奖完成
        /// </summary>
        public bool IsEnd { get; set; }

        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool? CanComment { get; set; } = true;

        /// <summary>
        /// 参加抽奖的前置条件
        /// </summary>
        public LotteryConditionType ConditionType { get; set; }

        /// <summary>
        /// 游戏SteamId （复用这个字段，用来保存与参与条件相关的信息）
        /// </summary>
        public string GameSteamId { get; set; }


        public virtual ICollection<LotteryUser> Users { get; set; } = new List<LotteryUser>();

        public virtual ICollection<LotteryAward> Awards { get; set; } = new List<LotteryAward>();


    }

    public enum LotteryType
    {
        [Display(Name = "手动")]
        Manual,
        [Display(Name = "自动")]
        Automatic,
        [Display(Name = "用户权重")]
        UserWeights,
    }

    public enum LotteryConditionType
    {
        [Display(Name = "无")]
        None,
        [Display(Name = "至少有一条游玩记录")]
        GameRecord,
        [Display(Name = "评论该抽奖")]
        CommentLottery,
        [Display(Name = "预约游戏")]
        BookingGame,
        [Display(Name = "将游戏加入Steam愿望单")]
        Wishlist,
        [Display(Name = "抽奖开始后创建了新的游玩记录")]
        NewGameRecord,
    }

    public class LotteryUser
    {
        public long Id { get; set; }

        public DateTime ParticipationTime { get; set; }

        public int Number { get; set; }

        public bool IsHidden { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }

        public Lottery Lottery { get; set; }
        public long? LotteryId { get; set; }

        public LotteryPrize LotteryPrize { get; set; }

        public LotteryAward LotteryAward { get; set; }
        public long? LotteryAwardId { get; set; }
    }

    /// <summary>
    /// 奖项 用于展示
    /// </summary>
    public class LotteryAward
    {
        public long Id { get; set; }

        public int Priority { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }

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

        public LotteryAwardType Type { get; set; }

        public Lottery Lottery { get; set; }
        public long? LotteryId { get; set; }
        /// <summary>
        /// 附加积分 全类型生效
        /// </summary>
        public int Integral { get; set; }

        public virtual ICollection<LotteryUser> WinningUsers { get; set; } = new List<LotteryUser>();

        public virtual ICollection<LotteryPrize> Prizes { get; set; } = new List<LotteryPrize>();
    }

    public enum LotteryAwardType
    {
        [Display(Name = "激活码")]
        ActivationCode,
        [Display(Name = "实物")]
        RealThing,
        [Display(Name = "用户积分")]
        Integral,
    }

    /// <summary>
    /// 真实的奖品 用于发放给用户 例如激活码
    /// </summary>
    public class LotteryPrize
    {
        public long Id { get; set; }

        public string Context { get; set; }

        public LotteryUser LotteryUser { get; set; }
        public long? LotteryUserId { get; set; }

        public LotteryAward LotteryAward { get; set; }
        public long? LotteryAwardId { get; set; }
    }
}
