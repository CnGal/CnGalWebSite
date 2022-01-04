using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class AddPhoneNumberModel
    {
        [Phone(ErrorMessage = "请输入有效的手机号码")]
        [StringLength(11, ErrorMessage = "手机号码的长度为11位")]
        [Display(Name = "手机号码")]
        public string Phone { get; set; }
        [Display(Name = "登录令牌")]
        public string LoginToken { get; set; }
    }
}
