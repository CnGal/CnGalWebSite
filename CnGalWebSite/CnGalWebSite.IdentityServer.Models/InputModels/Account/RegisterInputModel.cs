using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.InputModels.Account
{
    public class RegisterInputModel
    {
        [Required(ErrorMessage = "请输入电子邮箱")]
        [EmailAddress(ErrorMessage = "电子邮箱的格式不正确")]
        [Display(Name = "电子邮箱")]
        public string Email { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "确认密码")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致，请重新输入")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "用户名")]
        [Required(ErrorMessage = "请输入用户名")]
        [MaxLength(20)]
        public string Name { get; set; }

        public string ReturnUrl { get; set; }

        public HumanMachineVerificationResult VerifyResult { get; set; } = new HumanMachineVerificationResult();

    }
}
