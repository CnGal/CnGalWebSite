using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.FriendLinks
{
    public class FriendLinkViewModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 跳转链接
        /// </summary>
        public string Url { get; set; }

    }
}
