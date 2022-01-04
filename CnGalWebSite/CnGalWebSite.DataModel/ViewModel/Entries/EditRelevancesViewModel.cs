using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditRelevancesViewModel
    {
        public EntryType Type { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }

        public List<RelevancesModel> Roles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> staffs { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Groups { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Games { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> articles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> news { get; set; } = new List<RelevancesModel>();

        public List<RelevancesModel> others { get; set; } = new List<RelevancesModel>();

        [Display(Name = "备注")]
        public string Note { get; set; }

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
