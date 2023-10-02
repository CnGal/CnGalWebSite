using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Stalls
{
    public class StallInfoViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        /// <summary>
        /// 报价
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// 职位类型 大类
        /// </summary>
        public ProjectPositionType PositionType { get; set; }

        /// <summary>
        /// 职位类型名称
        /// </summary>
        public string PositionTypeName { get; set; }

        /// <summary>
        /// 职位类型 小类
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public UserInfoViewModel UserInfo { get; set; }=new UserInfoViewModel();
    }
}
