
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class UserOverviewModel
    {
        public string Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 个性签名
        /// </summary>
        public string PersonalSignature { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime RegistTime { get; set; }
        /// <summary>
        /// 总积分
        /// </summary>
        public int DisplayIntegral { get; set; }
        /// <summary>
        /// 总贡献值
        /// </summary>
        public int DisplayContributionValue { get; set; }
        /// <summary>
        /// 在线时长
        /// </summary>
        public double OnlineTime { get; set; }
        /// <summary>
        /// 最后在线时间
        /// </summary>
        public DateTime LastOnlineTime { get; set; }
        /// <summary>
        /// 可否留言
        /// </summary>
        public bool CanComment { get; set; }
    }

}
