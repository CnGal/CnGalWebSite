using System;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    [Obsolete("迁移至StoreInfo")]
    public class SteamInfor
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }

        public int SteamId { get; set; } = -1;

        public int OriginalPrice { get; set; }

        public int PriceNow { get; set; }
        public string PriceNowString { get; set; }
        public int CutNow { get; set; }

        /// <summary>
        /// 平均游玩时长 小时 不用分钟是因为之前就是double
        /// </summary>
        public double PlayTime { get; set; }

        public int PriceLowest { get; set; }
        public string PriceLowestString { get; set; }
        public int CutLowest { get; set; }

        /// <summary>
        /// 评测数
        /// </summary>
        public int EvaluationCount { get; set; }
        /// <summary>
        /// 好评率
        /// </summary>
        public int RecommendationRate { get; set; }

        /// <summary>
        /// 估计拥有人数 上限
        /// </summary>
        public int EstimationOwnersMax { get; set; }
        /// <summary>
        /// 估计拥有人数 下限
        /// </summary>
        public int EstimationOwnersMin { get; set; }

        /// <summary>
        /// 关联的游戏
        /// </summary>
        public Entry Entry { get; set; }
        public int? EntryId { get; set; }

        public DateTime LowestTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    public class SteamUserInfor
    {
        public long Id { get; set; }

        public string SteamId { get; set; }

        public string Image { get; set; }

        public string Name { get; set; }
    }

    public class SteamUserInforJson
    {
        public SteamUserInforResponseJson response { get; set; } = new SteamUserInforResponseJson();
    }
    public class SteamUserInforResponseJson
    {
        public List<SteamUserInforplayersJson> players { get; set; } = new List<SteamUserInforplayersJson>();
    }
    public class SteamUserInforplayersJson
    {
        public string steamid { get; set; }
        public string personaname { get; set; }
        public string profileurl { get; set; }
        public string avatarfull { get; set; }
    }
    public class SteamEvaluation
    {
        /// <summary>
        /// 评测数
        /// </summary>
        public int EvaluationCount { get; set; }
        /// <summary>
        /// 好评率
        /// </summary>
        public int RecommendationRate { get; set; }
    }

    public class SteamNowJson
    {
        public double? price { get; set; }
        public int? cut { get; set; }
        public string price_formatted { get; set; } = "¥ 0";
    }

    public class SteamLowestJson
    {
        public double? price { get; set; }
        public int? cut { get; set; }
        public string price_formatted { get; set; } = "¥ 0";
        public long recorded { get; set; }
    }
    public class UserSteamResponseJson
    {
        public int game_count { get; set; }
        public List<UserSteamGameJson> games { get; set; } = new List<UserSteamGameJson>();
    }

    public class UserSteamGameJson
    {
        public int appid { get; set; }
        public int playtime_forever { get; set; }
    }
}
