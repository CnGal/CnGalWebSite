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

        [Display(Name = "2DFan条目Id")]
        public string _2DFanId { get; set; }
        [Display(Name = "Bangumi条目Id")]
        public string BangumiId { get; set; }
        [Display(Name = "WikiData条目Id")]
        public string WikiDataId { get; set; }
        [Display(Name = "VNDB条目Id")]
        public string VNDBId { get; set; }
        [Display(Name = "月幕Galgame档案Id")]
        public string YMGalId { get; set; }
        [Display(Name = "萌娘百科条目名称")]
        public string MoegirlName { get; set; }
        [Display(Name = "百度百科条目名称")]
        public string BaiDuName { get; set; }
        [Display(Name = "中文维基百科条目名称")]
        public string ZhWikiPediaName { get; set; }
    }
    public class RelevancesModel
    {
        [Display(Name = "唯一名称")]
        //[Required(ErrorMessage ="请输入显示名称")]
        public string DisplayName { get; set; }

        [Display(Name = "简介")]
        // [Required(ErrorMessage = "请输入显示值")]
        public string DisPlayValue { get; set; }

        [Display(Name = "链接")]
        // [Required(ErrorMessage = "请输入完整链接")]
        public string Link { get; set; }
    }
}
