using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Models
{
    public class TokenCustom
    {
        public int Id { get; set; }
        //实际的字符串令牌
        public string Token { get; set; }
        //用户获取的6位数令牌
        public int? Num { get; set; }

        public string UserName { get; set; }

        public DateTime? Time { get; set; }
    }
    public enum TokenType
    {
        [Display(Name = "创建用户")]
        CreateUser,
        [Display(Name = "修改密码")]
        ChangePassward,
        [Display(Name = "修改修改电子邮箱第一步")]
        ChangeEmailBefore,
        [Display(Name = "修改修改电子邮箱第二步")]
        ChangeEmailAfter
    }

    public enum SMSType
    {
        Register,
        ForgetPassword,
        ChangeMobilePhoneNumber
    }
}
