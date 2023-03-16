using CnGalWebSite.IdentityServer.Models.Geetest;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class ResetPasswordInputModel
    {
        [Required(ErrorMessage = "请输入电子邮箱")]
        [EmailAddress(ErrorMessage = "电子邮箱的格式不正确")]
        [Display(Name = "电子邮箱")]
        public string Email { get; set; }

        public string ReturnUrl { get; set; }

        public HumanMachineVerificationResult VerifyResult { get; set; } = new HumanMachineVerificationResult();

    }
}
