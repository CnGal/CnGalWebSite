using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "请输入电子邮箱")]
        [EmailAddress]
        [Display(Name = "电子邮箱")]
        public string Email { get; set; }

        [Display(Name = "主页")]
        public string MainPageContext { get; set; }

        [Display(Name = "个性签名")]
        public string PersonalSignature { get; set; }
        [Display(Name = "生日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }

        public string PhotoName { get; set; }

        public string BackgroundName { get; set; }

        public string MBgImageName { get; set; }

        public string SBgImageName { get; set; }

        public List<string> Claims { get; set; }

        public IList<UserRolesModel> Roles { get; set; }
        [Display(Name = "积分")]
        public int Integral { get; set; }
        [Display(Name = "贡献值")]
        public int ContributionValue { get; set; }
        [Display(Name = "是否开启空间留言")]
        public bool CanComment { get; set; }
        [Display(Name = "是否公开收藏夹")]
        public bool IsShowFavotites { get; set; }
        [Display(Name = "是否通过身份验证")]
        public bool IsPassedVerification { get; set; }

        public EditUserViewModel()
        {
            Claims = new List<string>();
            Roles = new List<UserRolesModel>();
        }
    }
    public class UserRolesModel
    {
        public string Name { get; set; }

        public bool IsSelected { get; set; }
    }
}
