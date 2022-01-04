using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class ChangeEmailBeforeModel
    {

        [Display(Name = "身份验证密匙")]
        public string LoginKey { get; set; }
        [Display(Name = "新邮箱")]
        [EmailAddress(ErrorMessage = "请输入有效的电子邮箱")]
        public string NewEmail { get; set; }
    }
}
