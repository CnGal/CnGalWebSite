using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class ChangePhoneNumberAfterModel
    {
        [Required(ErrorMessage = "请输入验证码")]
        [Display(Name = "验证码")]
        public string NumToken { get; set; }
        [Display(Name = "身份验证密匙")]
        public string LoginKey { get; set; }
        [Phone(ErrorMessage = "请输入有效的手机号码")]
        [MaxLength(11, ErrorMessage = "手机号码的长度为11位")]
        [MinLength(11, ErrorMessage = "手机号码的长度为11位")]
        [Display(Name = "手机号码")]
        public string NewPhone { get; set; }
    }
}
