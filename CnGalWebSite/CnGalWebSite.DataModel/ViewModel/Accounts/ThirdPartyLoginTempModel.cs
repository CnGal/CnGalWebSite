using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class ThirdPartyLoginTempModel
    {
        [Display(Name = "第三方登录令牌")]
        public string ThirdLoginKey { get; set; }
        [Display(Name = "第三方登录类型")]
        public ThirdPartyLoginType Type { get; set; }
    }
}
