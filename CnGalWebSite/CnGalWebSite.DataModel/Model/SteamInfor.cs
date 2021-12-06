namespace CnGalWebSite.DataModel.Model
{
    public class SteamInfor
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }

        public int SteamId { get; set; } = -1;

        public int OriginalPrice { get; set; }

        public int PriceNow { get; set; }
        public string PriceNowString { get; set; }
        public int CutNow { get; set; }

        public double PlayTime { get; set; }

        public int PriceLowest { get; set; }
        public string PriceLowestString { get; set; }
        public int CutLowest { get; set; }
        /// <summary>
        /// 关联的游戏
        /// </summary>
        public Entry? Entry { get; set; }
        public int? EntryId { get; set; }

        public DateTime LowestTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    public class SteamNowJson
    {
        public int price { get; set; }
        public int cut { get; set; }
        public string price_formatted { get; set; } = "¥ 0";
    }

    public class SteamLowestJson
    {
        public int price { get; set; }
        public int cut { get; set; }
        public string price_formatted { get; set; } = "¥ 0";
        public long recorded { get; set; }
    }
    public class UserSteamResponseJson
    {
        public int game_count { get; set; }
        public List<UserSteamGameJson>? games { get; set; }
    }

    public class UserSteamGameJson
    {
        public int appid { get; set; }
        public int playtime_forever { get; set; }
    }
}
