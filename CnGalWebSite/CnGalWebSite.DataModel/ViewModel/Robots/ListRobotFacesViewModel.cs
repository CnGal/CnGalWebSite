using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Robots
{
    public class ListRobotFacesViewModel
    {
        public List<ListRobotFaceAloneModel> RobotFaces { get; set; }
    }
    public class ListRobotFaceAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "匹配关键字")]
        public string Key { get; set; }
        [Display(Name = "消息")]
        public string Value { get; set; }
        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
    }

    public class RobotFacesPagesInfor
    {
        public Search.QueryPageOptions Options { get; set; }
        public ListRobotFaceAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
