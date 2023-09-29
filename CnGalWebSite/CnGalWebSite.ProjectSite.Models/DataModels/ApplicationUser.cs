using Microsoft.AspNetCore.Identity;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// 个人介绍
        /// </summary>
        public string BriefIntroduction { get; set; }

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
    }

   
}
