using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CnGalWebSite.IdentityServer.Models.InputModels.Account
{
    public class AddPhoneNumberInputModel
    {
        public string UserId { get; set; }

        [Required(ErrorMessage = "请输入手机号")]
        //[RegularExpression(@"^1(3[0-9]|4[01456879]|5[0-35-9]|6[2567]|7[0-8]|8[0-9]|9[0-35-9])\d{8}$", ErrorMessage = "手机号格式不正确")]
        public string PhoneNumber { get; set; }

        public string Country { get; set; } = "86";

        public string Code { get; set; }
        public string SecondCode { get; set; }
        public string ReturnUrl { get; set; }

        public HumanMachineVerificationResult VerifyResult { get; set; } = new HumanMachineVerificationResult();
    }

 
}
