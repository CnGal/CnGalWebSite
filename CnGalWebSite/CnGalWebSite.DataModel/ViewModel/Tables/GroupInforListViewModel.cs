using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Tables
{

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

        [Display(Name = "B站")]
        public string Bilibili { get; set; }
        [Display(Name = "微博")]
        public string MicroBlog { get; set; }
    }
}
