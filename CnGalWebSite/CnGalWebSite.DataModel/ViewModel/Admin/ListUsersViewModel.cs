
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListUsersInforViewModel
    {
        public List<IdentityRole> Roles { get; set; }

        public int UserCount { get; set; }
    }

    public class ListUsersViewModel
    {
        public List<ListUserAloneModel> Users { get; set; }
    }
    public class ListUserAloneModel
    {
        [Display(Name = "Id")]
        public string Id { get; set; }
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "邮箱")]
        public string Email { get; set; }

        [Display(Name = "个性签名")]
        public string PersonalSignature { get; set; }
        [Display(Name = "生日")]
        public DateTime? Birthday { get; set; }
        [Display(Name = "注册时间")]
        public DateTime RegistTime { get; set; }
        [Display(Name = "总积分")]
        public int DisplayIntegral { get; set; }
        [Display(Name = "总贡献值")]
        public int DisplayContributionValue { get; set; }

        [Display(Name = "在线时长 小时")]
        public double OnlineTime { get; set; }
        [Display(Name = "最后在线时间")]
        public DateTime LastOnlineTime { get; set; }
        [Display(Name = "解封时间")]
        public DateTime? UnsealTime { get; set; }
        [Display(Name = "可否留言")]
        public bool CanComment { get; set; }
        [Display(Name = "是否通过验证")]
        public bool IsPassedVerification { get; set; }
        [Display(Name = "是否公开收藏")]
        public bool IsShowFavotites { get; set; }

    }

    public class UsersPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListUserAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }

}
