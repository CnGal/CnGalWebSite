using CnGalWebSite.ProjectSite.Models.ViewModels.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Users
{
    public class UserSpaceViewModel
    {
        /// <summary>
        /// tab栏索引缓存
        /// </summary>
        public int TabIndex { get; set; }

        /// <summary>
        /// 用户基础信息
        /// </summary>
        public UserInfoViewModel UserInfo { get; set; }=new UserInfoViewModel();

        /// <summary>
        /// 企划列表
        /// </summary>
        public List<ProjectInfoViewModel> projects { get; set; }=new List<ProjectInfoViewModel>();
    }
}
