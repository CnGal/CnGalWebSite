
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListFavoriteFoldersInforViewModel
    {
        public long Defaults { get; set; }
        public long All { get; set; }
    }

    public class ListFavoriteFoldersViewModel
    {
        public List<ListFavoriteFolderAloneModel> FavoriteFolders { get; set; }
    }
    public class ListFavoriteFolderAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "关联用户Id")]
        public string UserId { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }
        [Display(Name = "收藏数")]
        public long Count { get; set; }

        [Display(Name = "是否默认")]
        public bool IsDefault { get; set; }
    }

    public class FavoriteFoldersPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListFavoriteFolderAloneModel SearchModel { get; set; }

        public string UserId { get; set; }
    }
}
