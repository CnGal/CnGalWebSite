using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Robots
{
    public class ListRobotGroupsViewModel
    {
        public List<ListRobotGroupAloneModel> RobotGroups { get; set; }
    }
    public class ListRobotGroupAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "群号")]
        public long GroupId { get; set; }

        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "强制匹配")]
        public bool ForceMatch { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
    }

    public class RobotGroupsPagesInfor
    {
        public Search.QueryPageOptions Options { get; set; }
        public ListRobotGroupAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
