using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class ThirdPartyLoginModel
    {
        [Display(Name = "代码")]
        public string Code { get; set; }
        [Display(Name = "返回链接")]
        public string ReturnUrl { get; set; }
        [Display(Name = "是否服务端")]
        public bool IsSSR { get; set; }
        [Display(Name = "第三方登录类型")]
        public ThirdPartyLoginType Type { get; set; }
    }
    public class ThirdPartyLoginResult
    {
        public ThirdPartyLoginResultType Code { get; set; }
        /// <summary>
        /// 错误描述
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// 成功后的登入令牌
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 身份验证通过的令牌
        /// </summary>
        public string ThirdLoginKey { get; set; }
        /// <summary>
        /// 成功登入的用户名
        /// </summary>
        public string UserName { get; set; }
    }
    public enum ThirdPartyLoginResultType
    {
        Failed,
        LoginSuccessed,
        NoAssociatedAccount,
        RepeatRequest


    }

}
