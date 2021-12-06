using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class EditFriendLinksViewModel
    {

        public List<FriendLinkModel> FriendLinks { get; set; }
    }
    public class FriendLinkModel
    {
        [Display(Name = "图片链接")]
        [Required(ErrorMessage = "请填写图片链接")]
        public string ImagePath { get; set; }

        [Display(Name = "链接")]
        [Required(ErrorMessage = "请填写链接")]
        public string Link { get; set; }

        [Display(Name = "名称")]
        [Required(ErrorMessage = "请填写名称")]
        public string Name { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; } = 0;

    }
}
