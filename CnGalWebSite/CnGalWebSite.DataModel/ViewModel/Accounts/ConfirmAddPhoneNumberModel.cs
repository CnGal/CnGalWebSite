using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class ConfirmAddPhoneNumberModel
    {
        [Display(Name = "手机号")]
        public string Phone { get; set; }
        [Display(Name = "登录令牌")]
        public string LoginToken { get; set; }

        [Display(Name = "验证码")]
        public string NumToken { get; set; }
    }
}
