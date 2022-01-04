using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class CheckVerificationCodeModel
    {
        [Display(Name = "验证码")]
        public string Num { get; set; }
    }
}
