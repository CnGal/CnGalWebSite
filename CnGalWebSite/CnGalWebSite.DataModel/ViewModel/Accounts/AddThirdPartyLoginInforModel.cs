using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class AddThirdPartyLoginInforModel
    {
        [Display(Name = "登录令牌")]
        public string LoginKey { get; set; }
        [Display(Name = "第三方登录令牌")]
        public string ThirdPartyKey { get; set; }
        [Display(Name = "第三方登录类型")]
        public ThirdPartyLoginType Type { get; set; }
    }
}
