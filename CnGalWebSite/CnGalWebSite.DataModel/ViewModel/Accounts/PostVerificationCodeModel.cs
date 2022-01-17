using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class PostVerificationCodeModel
    {
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "登录令牌")]
        public string LoginKey { get; set; }
        [Display(Name = "邮件")]
        public string Mail { get; set; }
        [Display(Name = "短信类型")]
        public SMSType SMSType { get; set; }
        [Display(Name = "发送类型")]
        public SendType SendType { get; set; }
        [Display(Name = "Token")]
        public string Token { get; set; }
        [Display(Name = "Randstr")]
        public string Randstr { get; set; }
        [Display(Name = "Challenge")]
        public string Challenge { get; set; }
        [Display(Name = "Validate")]
        public string Validate { get; set; }
        [Display(Name = "Seccode")]
        public string Seccode { get; set; }


    }
}
