using CnGalWebSite.IdentityServer.Models.Account;
using System;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.Messages
{
    public class SendRecord
    {
        public long Id { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// 目标地址
        /// </summary>
        public string Address { get; set; }

        public DateTime Time { get; set; }

        public SendRecordType Type { get; set; }

        public SendRecordPurpose Purpose { get; set; }
    }

    public enum SendRecordType
    {
        Email,
        SMS
    }

    public enum SendRecordPurpose
    {
        [Display(Name = "账号注册")]
        Register,
        [Display(Name = "重置密码")]
        ResetPassword,
        [Display(Name = "绑定手机")]
        AddPhoneNumber,
        [Display(Name = "换绑手机")]
        ChangePhoneNumber,
        [Display(Name = "换绑邮箱")]
        ChangeEmail
    }
}
