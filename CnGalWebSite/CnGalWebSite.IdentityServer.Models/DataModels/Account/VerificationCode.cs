using System.ComponentModel.DataAnnotations;
using System;
using CnGalWebSite.IdentityServer.Models.DataModels.Messages;

namespace CnGalWebSite.IdentityServer.Models.DataModels.Account
{
    public class VerificationCode
    {
        public long Id { get; set; }
        /// <summary>
        /// 实际的字符串令牌
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 用户获取的6位数令牌
        /// </summary>
        public int Code { get; set; }

        public int FailedCount { get; set; }

        public string UserId { get; set; }

        public DateTime Time { get; set; }

        public VerificationCodeType Type { get; set; }

    }

    public enum VerificationCodeType
    {
        [Display(Name = "账号注册")]
        Register,
        [Display(Name = "重置密码")]
        ResetPassword,
        [Display(Name = "绑定手机")]
        AddPhoneNumber,
        [Display(Name = "二次身份验证")]
        SecondVerify,
        [Display(Name = "换绑手机")]
        ChangePhoneNumber,
        [Display(Name = "换绑电子邮箱")]
        ChangeEmail,
        [Display(Name = "密码验证通过")]
        VerifyPassword,
    }

    public static class VerificationCodeTypeHelper
    {
        public static SendRecordPurpose ToPurpose(this VerificationCodeType type)
        {
            return type switch
            {
                VerificationCodeType.Register => SendRecordPurpose.Register,
                VerificationCodeType.ResetPassword => SendRecordPurpose.ResetPassword,
                VerificationCodeType.AddPhoneNumber => SendRecordPurpose.AddPhoneNumber,
                VerificationCodeType.SecondVerify => SendRecordPurpose.SecondVerify,
                VerificationCodeType.ChangePhoneNumber => SendRecordPurpose.ChangePhoneNumber,
                VerificationCodeType.ChangeEmail => SendRecordPurpose.ChangeEmail,
                _ => throw new NotImplementedException()
            };
        }

        public static bool IsPhoneNumber(this VerificationCodeType type)
        {
            return type == VerificationCodeType.AddPhoneNumber || type == VerificationCodeType.ChangePhoneNumber;
        }
    }
}
