using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Users
{
    public class UserOverviewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

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
        /// 标签
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string Email { get; set; }
    }
}
