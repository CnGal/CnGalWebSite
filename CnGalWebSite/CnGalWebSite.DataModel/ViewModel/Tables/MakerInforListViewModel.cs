using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Tables
{
    public class MakerInforTableModel
    {
        public long Id { get; set; }
        [Display(Name = "Id")]
        public long RealId { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "别称")]
        public string AnotherName { get; set; }
   
        [Display(Name = "B站")]
        public string Bilibili { get; set; }
        [Display(Name = "微博")]
        public string MicroBlog { get; set; }
        #region 已删除

        [Display(Name = "昵称（官方称呼）")]
        [Obsolete("昵称已删除")]
        public string Nickname { get; set; }
        [Display(Name = "博客")]
        [Obsolete("不再统计博客")]
        public string Blog { get; set; }
        [Display(Name = "Lofter")]
        [Obsolete("不再统计Lofter")]
        public string Lofter { get; set; }
        [Display(Name = "其他活动网站")]
        [Obsolete("不再统计其他活动网站")]
        public string Other { get; set; }

        #endregion
     
    }
}
