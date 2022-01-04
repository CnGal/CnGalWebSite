using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Tables
{
    public class StaffInforListViewModel
    {
        public List<StaffInforTableModel> StaffInfors { get; set; }
    }

    public class StaffInforTableModel
    {
        public long Id { get; set; }
        [Display(Name = "Id")]
        public long RealId { get; set; }
        [Display(Name = "游戏名称")]
        public string GameName { get; set; }
        [Display(Name = "分组")]
        public string Subcategory { get; set; }
        [Display(Name = "官方职位")]
        public string PositionOfficial { get; set; }
        [Display(Name = "官方称呼")]
        public string NicknameOfficial { get; set; }
        [Display(Name = "通用职位")]
        public PositionGeneralType PositionGeneral { get; set; }
        [Display(Name = "角色")]
        public string Role { get; set; }
        [Display(Name = "隶属组织")]
        public string SubordinateOrganization { get; set; }
    }
}
