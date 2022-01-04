using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Tables
{
    public class SteamInforListViewModel
    {
        public List<SteamInforTableModel> SteamInfors { get; set; }
    }

    public class SteamInforTableModel
    {
        public long Id { get; set; }
        [Display(Name = "词条Id")]
        public long EntryId { get; set; }
        [Display(Name = "游戏名称")]
        public string Name { get; set; }
        [Display(Name = "SteamId")]
        public int SteamId { get; set; } = -1;
        [Display(Name = "原价￥")]
        public int OriginalPrice { get; set; }
        [Display(Name = "现价￥")]
        public int PriceNow { get; set; }
        [Display(Name = "当前折扣 %")]
        public int CutNow { get; set; }
        [Display(Name = "历史最低价￥")]
        public int PriceLowest { get; set; }
        [Display(Name = "历史最高折扣 %")]
        public int CutLowest { get; set; }
        [Display(Name = "史低价格出现时间")]
        public DateTime LowestTime { get; set; }
    }
}
