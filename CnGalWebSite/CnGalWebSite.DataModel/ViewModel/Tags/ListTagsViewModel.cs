
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class ListTagsInforViewModel
    {
        public int Hiddens { get; set; }
        public int All { get; set; }
    }

    public class ListTagsViewModel
    {
        public List<ListTagAloneModel> Tags { get; set; }
    }
    public class ListTagAloneModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "最后编辑时间")]
        public DateTime LastEditTime { get; set; }
        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; } = false;
    }

    public class TagsPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListTagAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
