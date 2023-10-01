using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// 个人介绍
        /// </summary>
        public string PersonDescription { get; set; }

        /// <summary>
        /// 组织介绍
        /// </summary>
        public string OrganizationDescription { get; set; }

        /// <summary>
        /// 个人网名
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime RegistTime { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 用户空间头图
        /// </summary>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// 身份
        /// </summary>
        public UserType Type { get; set; }

        /// <summary>
        /// 企划
        /// </summary>
        public List<Project> Projects { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool Hide { get; set; }
    }

    public enum UserType
    {
        [Display(Name ="创作者")]
        Person,
        [Display(Name ="组织")]
        Organization
    }
   
}
