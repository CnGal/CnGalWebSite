using IdentityServer4.Models;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class AddPhoneNumberInputModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "请输入手机号")]
        [RegularExpression(@"^1(3[0-9]|4[01456879]|5[0-35-9]|6[2567]|7[0-8]|8[0-9]|9[0-35-9])\d{8}$",ErrorMessage="手机号格式不正确")]
        public string PhoneNumber { get; set; }

        public string ReturnUrl { get; set; }
    }
}
