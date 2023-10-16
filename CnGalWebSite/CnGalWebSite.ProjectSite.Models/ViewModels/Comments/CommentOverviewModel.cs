using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Comments
{
    public class CommentOverviewModel
    {
        public long Id { get; set; }

        public DateTime CreateTime { get; set; }

        public CommentType Type { get; set; }

        public string Text { get; set; }

        public string UserId {  get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        public PageType PageType { get; set; }

        public long PageId { get; set; }


    }
}
