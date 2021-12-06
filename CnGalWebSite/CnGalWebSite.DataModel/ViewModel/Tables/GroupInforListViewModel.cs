using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Tables
{
    public class GroupInforListViewModel
    {
        public List<GroupInforTableModel> GroupInfors { get; set; }
    }

    public class GroupInforTableModel
    {
        public long Id { get; set; }
        [Display(Name = "Id")]
        public long RealId { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "QQ群")]
        public string QQgroupGroup { get; set; }
        [Display(Name = "别称")]
        public string AnotherNameGroup { get; set; }
    }
}
