using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Users
{
    public class UserInfoViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

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

        public string Contact { get; set; }


        public string GetName(UserType? type=null)
        {
            if ((type??Type) == UserType.Organization)
            {
                return string.IsNullOrWhiteSpace(OrganizationName) ? Name : OrganizationName;
            }
            else
            {
                return string.IsNullOrWhiteSpace(PersonName) ? Name : PersonName;
            }
        }
    }
}
