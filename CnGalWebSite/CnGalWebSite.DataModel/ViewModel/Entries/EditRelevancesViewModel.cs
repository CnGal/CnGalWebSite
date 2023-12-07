using CnGalWebSite.DataModel.ViewModel.Entries;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditRelevancesViewModel : BaseEntryEditModel
    {
        public List<RelevancesModel> Roles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> staffs { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Groups { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Games { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> articles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> news { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> videos { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> others { get; set; } = new List<RelevancesModel>();
    }
    public class RelevancesModel
    {
        [Display(Name = "唯一名称")]
        public string DisplayName { get; set; }

        [Display(Name = "简介")]
        public string DisPlayValue { get; set; }

        [Display(Name = "链接")]
        public string Link { get; set; }
    }

    public enum OutlinkType
    {
        [Display(Name = "官网")]
        Official,
        [Display(Name = "微博")]
        Weibo,
        [Display(Name = "Bilibili")]
        Bilibili,
        [Display(Name = "AcFun")]
        AcFun,
        [Display(Name = "知乎")]
        Zhihu,
        [Display(Name = "爱发电")]
        Afdian,
        [Display(Name = "摩点")]
        Modian,
        [Display(Name = "Pixiv")]
        Pixiv,
        [Display(Name = "Twitter")]
        Twitter,
        [Display(Name = "YouTube")]
        YouTube,
        [Display(Name = "Facebook")]
        Facebook,
        [Display(Name = "2DFan")]
        _2DFan,
        [Display(Name = "Bangumi")]
        Bangumi,
        [Display(Name = "VNDB")]
        VNDB,
        [Display(Name = "月幕Galgame")]
        YMGal,
        [Display(Name = "小黑盒")]
        XiaoHeiHe,
        [Display(Name = "WikiData")]
        WikiData,
        [Display(Name = "萌娘百科")]
        Moegirl,
        [Display(Name = "百度百科")]
        BaiDu,
        [Display(Name = "中文维基百科")]
        ZhWikiPedia,
    }
}
