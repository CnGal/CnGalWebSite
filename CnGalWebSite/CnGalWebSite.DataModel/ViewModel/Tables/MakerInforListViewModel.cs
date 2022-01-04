using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Tables
{
    public class MakerInforListViewModel
    {
        public List<MakerInforTableModel> MakerInfors { get; set; }
    }
    public class MakerInforTableModel
    {
        public long Id { get; set; }
        [Display(Name = "Id")]
        public long RealId { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "别称")]
        public string AnotherName { get; set; }
        [Display(Name = "昵称（官方称呼）")]
        public string Nickname { get; set; }
        [Display(Name = "Bilibili")]
        public string Bilibili { get; set; }
        [Display(Name = "微博")]
        public string MicroBlog { get; set; }
        [Display(Name = "博客")]
        public string Blog { get; set; }
        [Display(Name = "Lofter")]
        public string Lofter { get; set; }
        [Display(Name = "其他活动网站")]
        public string Other { get; set; }
    }
}
