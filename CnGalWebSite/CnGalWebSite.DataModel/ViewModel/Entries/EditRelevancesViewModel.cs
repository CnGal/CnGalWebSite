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
        public List<RelevancesModel> others { get; set; } = new List<RelevancesModel>();
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
