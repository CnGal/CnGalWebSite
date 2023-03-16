using CnGalWebSite.IdentityServer.Models.Geetest;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class VerifyCodeInputModel
    {
        [Required(ErrorMessage ="请输入验证码")]
        [StringLength(6,ErrorMessage = "验证码长度为6")]
        public string Code { get; set; }

        public string UserId { get; set; }

        public string NewEmail { get; set; }
        public string NewPhoneNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string SecondCode { get; set; }

        public VerificationCodeType Type { get; set; }

        public string ReturnUrl { get; set; }

        public HumanMachineVerificationResult VerifyResult { get; set; } = new HumanMachineVerificationResult();
    }
}
