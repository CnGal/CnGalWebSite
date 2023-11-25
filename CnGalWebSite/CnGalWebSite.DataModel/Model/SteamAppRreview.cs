

using System;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.Model
{
    public class SteamAppRreview
    {
        public int Id { get; set; }
        /// <summary>
        /// 推荐的唯一 ID
        /// </summary>
        public int recommendationid {  get; set; }

        /// <summary>
        /// 用户的 SteamID
        /// </summary>
        public string steamid {  get; set; }

        /// <summary>
        /// 用户拥有的游戏数量
        /// </summary>
        public long? num_games_owned {  get; set; }

        /// <summary>
        /// 用户撰写的评测数量
        /// </summary>
        public long? num_reviews { get; set; }

        /// <summary>
        /// 此应用所记录的总计游戏时间
        /// </summary>
        public long? playtime_forever { get; set; }

        /// <summary>
        /// 此应用所记录的过去两周的游戏时间
        /// </summary>
        public long? playtime_last_two_weeks { get; set; }

        /// <summary>
        /// 撰写评测时的游戏时间
        /// </summary>
        public long? playtime_at_review { get; set; }

        /// <summary>
        /// 用户上次游戏的时间
        /// </summary>
        public long? last_played { get; set; }

        /// <summary>
        /// 用户撰写评测时使用的语言
        /// </summary>
        public string language { get; set; }

        /// <summary>
        /// 评测文本
        /// </summary>
        [StringLength(10000000)]
        public string review { get; set; }

        /// <summary>
        ///评测的创建日期（ Unix 时间戳） 
        /// </summary>
        public long? timestamp_created { get; set; }

        /// <summary>
        /// 上次更新评测的日期（ Unix 时间戳）
        /// </summary>
        public long? timestamp_updated { get; set; }

        /// <summary>
        /// 是一则正面推荐
        /// </summary>
        public bool? voted_up { get; set; }

        /// <summary>
        /// 认为这篇评测有价值的用户人数
        /// </summary>
        public long? votes_up { get; set; }

        /// <summary>
        /// 认为这篇评测欢乐的用户人数
        /// </summary>
        public long? votes_funny { get; set; }

        /// <summary>
        /// 价值分数
        /// </summary>
        public string weighted_vote_score { get;set; }

        /// <summary>
        /// 这篇评测下的留言数
        /// </summary>
        public long? comment_count { get; set; }

        /// <summary>
        /// 用户在 Steam 上购买了此游戏
        /// </summary>
        public bool? steam_purchase { get; set; }
        /// <summary>
        /// 用户勾选了该应用为免费获得的复选框
        /// </summary>
        public bool? received_for_free { get; set; }
        /// <summary>
        /// 用户是在抢先体验阶段发表的评测
        /// </summary>
        public bool? written_during_early_access { get; set; }

        public bool? hidden_in_steam_china { get; set; }

        public string steam_china_location { get; set; }

        /// <summary>
        /// 开发者回复文本
        /// </summary>
        [StringLength(10000000)]
        public string developer_response { get; set; }

        /// <summary>
        /// 开发者回复的 Unix 时间戳
        /// </summary>
        public long? timestamp_dev_responded { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        public long appid { get; set; }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime update_time { get; set; }

    }
}
