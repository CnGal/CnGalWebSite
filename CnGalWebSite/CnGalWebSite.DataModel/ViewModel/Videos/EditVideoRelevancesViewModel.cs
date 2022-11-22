using CnGalWebSite.DataModel.ViewModel.Entries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Videos
{
    public class EditVideoRelevancesViewModel : BaseEntryEditModel
    {
        public List<RelevancesModel> Roles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> staffs { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Groups { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Games { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> articles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> news { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> videos { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> others { get; set; } = new List<RelevancesModel>();

        [Display(Name = "B站视频Id（bvid/aid）")]
        public string BilibiliId { get; set; }
    }
}
