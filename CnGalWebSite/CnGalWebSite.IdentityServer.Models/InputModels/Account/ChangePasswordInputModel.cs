using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.InputModels.Account
{
    public class ChangePasswordInputModel
    {
        [Required(ErrorMessage = "请输入旧密码")]
        [DataType(DataType.Password)]
        [Display(Name = "旧密码")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "确认密码")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "两次输入的密码不一致，请重新输入")]
        public string ConfirmNewPassword { get; set; }

        public string SecondCode { get; set; }
        public string ReturnUrl { get; set; }
    }
}
