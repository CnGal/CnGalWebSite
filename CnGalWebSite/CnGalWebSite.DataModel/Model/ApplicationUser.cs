using CnGalWebSite.DataModel.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.Model
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(20000000)]
        public string MainPageContext { get; set; } = "### 哇，这里什么都没有呢";

        public string PersonalSignature { get; set; } = "哇，这里什么都没有呢";

        public DateTime? Birthday { get; set; }

        public DateTime RegistTime { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string PhotoPath { get; set; }
        /// <summary>
        /// 用户空间头图
        /// </summary>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// 用户背景图 大屏幕
        /// </summary>
        public string MBgImage { get; set; }

        /// <summary>
        /// 用户背景图 小屏幕
        /// </summary>
        public string SBgImage { get; set; }

        /// <summary>
        /// 上次修改密码时间
        /// </summary>
        public DateTime? LastChangePasswordTime { get; set; }
        /// <summary>
        /// 绑定的群聊QQ号
        /// </summary>
        public long GroupQQ { get; set; }
        /// <summary>
        /// Steam账户
        /// </summary>
        public string SteamId { get; set; }
        /// <summary>
        /// 上次更新steam信息时间
        /// </summary>
        public DateTime LastUpdateSteamInfoTime { get; set; }
        /// <summary>
        /// 附加积分
        /// </summary>
        [Obsolete("此项已不计入积分统计，请在积分列表中添加")]
        public int Integral { get; set; } = 0;
        /// <summary>
        /// 附加贡献值
        /// </summary>
        [Obsolete("此项已不计入积分统计，请在积分列表中添加")]
        public int ContributionValue { get; set; } = 0;
        /// <summary>
        /// G币 缓存项，请向积分列表添加
        /// </summary>
        public int GCoins { get; set; }
        /// <summary>
        /// 显示积分 = 附加 + 计算
        /// </summary>
        public int DisplayIntegral { get; set; } = 0;
        /// <summary>
        /// 显示贡献值 = 附加 + 计算
        /// </summary>
        public int DisplayContributionValue { get; set; } = 0;
        /// <summary>
        /// 在线时间 单位 秒
        /// </summary>
        public long OnlineTime { get; set; } = 0;
        /// <summary>
        /// 最后在线时间
        /// </summary>
        public DateTime LastOnlineTime { get; set; }
        /// <summary>
        /// 解封时间
        /// </summary>
        public DateTime? UnsealTime { get; set; }
        /// <summary>
        /// 是否可以留言
        /// </summary>
        public bool? CanComment { get; set; } = true;
        /// <summary>
        /// 是否公开收藏夹
        /// </summary>
        [Obsolete("用户主页展示设置为公开的所有收藏夹，不受此项限制")]
        public bool IsShowFavotites { get; set; }
        /// <summary>
        /// 是否公开游玩记录
        /// </summary>
        public bool IsShowGameRecord { get; set; }

        /// <summary>
        /// 是否通过身份验证
        /// </summary>
        public bool IsPassedVerification { get; set; }
        /// <summary>
        /// 管理用户创建的文件
        /// </summary>
        public FileManager FileManager { get; set; }

        /// <summary>
        /// 管理用户空间的留言
        /// </summary>
        public UserSpaceCommentManager UserSpaceCommentManager { get; set; }

        /// <summary>
        /// 用户地址 用于接收奖品
        /// </summary>
        public UserAddress UserAddress { get; set; }
        /// <summary>
        /// 用户认证
        /// </summary>
        public UserCertification Certification { get; set; }

        public virtual ICollection<LotteryUser> Lotteries { get; set; }

        /// <summary>
        /// 积分 贡献值 列表
        /// </summary>
        public virtual ICollection<UserIntegral> Integrals { get; set; }

        public ICollection<SignInDay> SignInDays { get; set; }

        public ICollection<Examine> Examines { get; set; }

        public ICollection<Message> Messages { get; set; }

        public ICollection<PlayedGame> PlayedGames { get; set; }

        public ICollection<UserOnlineInfor> UserOnlineInfors { get; set; }

        public ICollection<FavoriteFolder> FavoriteFolders { get; set; }

        public ICollection<ThirdPartyLoginInfor> ThirdPartyLoginInfors { get; set; }

        public ICollection<RankUser> UserRanks { get; set; }

        public ICollection<PeripheryRelevanceUser> UserOwnedPeripheries { get; set; }

        public ICollection<OperationRecord> OperationRecords { get; set; }

        public virtual ICollection<UserMonitor> Monitors { get; set; }

        public virtual ICollection<UserReviewEditRecord> ReviewEntries { get; set; }

        public virtual ICollection<Commodity> Commodities { get; set; }
        public virtual ICollection<CommodityApplicationUser> CommodityApplicationUsers { get; set; }

    }

    public class UserCertification
    {
        public long Id { get; set; }

        public DateTime CertificationTime { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }
    }

    public class UserMonitor
    {
        public long Id { get; set; }

        public DateTime CreateTime { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }
    }

    public class UserIntegral
    {
        public long Id { get; set; }

        public int Count { get; set; }

        public string Note { get; set; }

        public UserIntegralType Type { get; set; }

        public UserIntegralSourceType SourceType { get; set; }

        public DateTime Time { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }
    }

    public enum UserIntegralSourceType
    {
        [Display(Name = "其他")]
        Other,
        [Display(Name = "绑定SteamId")]
        BindSteamId,
        [Display(Name = "抽奖")]
        Lottery,
        [Display(Name = "购买商品")]
        BuyCommodity
    }

    public enum UserIntegralType
    {
        [Display(Name = "经验值")]
        Integral,
        [Display(Name = "贡献值")]
        ContributionValue,
        [Display(Name = "G币")]
        GCoins,
    }

    public class SignInDay
    {
        public long Id { get; set; }

        public DateTime Time { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

    }

    public class UserAddress
    {
        public long Id { set; get; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string RealName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }
    }

}
