using System;
#if NET5_0_OR_GREATER
#else
#endif
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


        public int PriceLowest { get; set; }
        public string PriceLowestString { get; set; }
        public int CutLowest { get; set; }


        public DateTime LowestTime { get; set; }

        public DateTime UpdateTime { get; set; }
    }

    public class SteamNowJson
    {
        public int Price { get; set; }
        public int Cut { get; set; }
        public string Price_formatted { get; set; }
    }

    public class SteamLowestJson
    {
        public int Price { get; set; }
        public int Cut { get; set; }
        public string Price_formatted { get; set; }
        public long Recorded { get; set; }
    }
}
