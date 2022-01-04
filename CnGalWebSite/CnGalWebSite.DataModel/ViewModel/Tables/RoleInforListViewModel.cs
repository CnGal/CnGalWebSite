using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Tables
{
    public class RoleInforListViewModel
    {
        public List<RoleInforTableModel> RoleInfors { get; set; }= new List<RoleInforTableModel>();
    }

    public class RoleInforTableModel
    {
        public long Id { get; set; }
        [Display(Name = "Id")]
        public long RealId { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "声优")]
        public string CV { get; set; }
        [Display(Name = "别称")]
        public string AnotherNameRole { get; set; }
        [Display(Name = "性别")]
        public GenderType Gender { get; set; }
        [Display(Name = "身材数据")]
        public string FigureData { get; set; }
        [Display(Name = "身材(主观)")]
        public string FigureSubjective { get; set; }
        [Display(Name = "生日")]
        public DateTime? Birthday { get; set; }
        [Display(Name = "发色")]
        public string Haircolor { get; set; }
        [Display(Name = "瞳色")]
        public string Pupilcolor { get; set; }
        [Display(Name = "服饰")]
        public string ClothesAccessories { get; set; }
        [Display(Name = "性格")]
        public string Character { get; set; }
        [Display(Name = "角色身份")]
        public string RoleIdentity { get; set; }
        [Display(Name = "血型")]
        public string BloodType { get; set; }
        [Display(Name = "身高")]
        public string RoleHeight { get; set; }
        [Display(Name = "兴趣")]
        public string RoleTaste { get; set; }
        [Display(Name = "年龄")]
        public string RoleAge { get; set; }
        [Display(Name = "登场游戏")]
        public string GameName { get; set; }

    }

}
