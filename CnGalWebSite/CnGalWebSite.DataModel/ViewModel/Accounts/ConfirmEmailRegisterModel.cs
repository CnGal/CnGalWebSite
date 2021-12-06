using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.Model
{
    public class ConfirmEmailRegisterModel
    {
        [Required(ErrorMessage = "请输入验证码")]
        [Display(Name = "验证码")]
        public string NumToken { get; set; }
        [Display(Name = "用户名")]
        public string UserName { get; set; }
    }
    public class ConfirmEmailRegisterResult
    {
        public bool Successful { get; set; }
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
        public string LoginKey { get; set; }
    }
}
