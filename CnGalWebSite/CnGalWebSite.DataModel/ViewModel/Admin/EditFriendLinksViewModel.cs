using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class EditFriendLinksViewModel
    {

        public List<FriendLinkModel> FriendLinks { get; set; }
    }
    public class FriendLinkModel: BaseImageEditModel
    {
        [Display(Name = "链接")]
        [Required(ErrorMessage = "请填写链接")]
        public string Link { get; set; }

        [Display(Name = "名称")]
        [Required(ErrorMessage = "请填写名称")]
        public string Name { get; set; }
    }
}
