using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Comments
{
    public class CommentViewModel
    {
        public long Id { get; set; }

        public DateTime CreateTime { get; set; }

        public CommentType Type { get; set; }

        public string Text { get; set; }

        public UserInfoViewModel UserInfor { get; set; } = new UserInfoViewModel();

        /// <summary>
        /// 子评论
        /// </summary>
        public List<CommentViewModel> Children { get; set; }=new List<CommentViewModel>();
    }
}
