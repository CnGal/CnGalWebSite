using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class HistorUserChangePasswordModel
    {
        [Required(ErrorMessage = "请输入验证码")]
        [Display(Name = "验证码")]
        public string NumToken { get; set; }

        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }
        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "确认密码")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致，请重新输入")]
        public string ConfirmPassword { get; set; }
    }
}
