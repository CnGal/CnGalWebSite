
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Coments
{
    public class CommentViewModel
    {
        public long Id { get; set; }

        public DateTime CommentTime { get; set; }

        public CommentType Type { get; set; }

        public string Text { get; set; }

        public UserInforViewModel UserInfor { get; set; } = new UserInforViewModel();

        /// <summary>
        /// 子评论
        /// </summary>
        public List<CommentViewModel> InverseParentCodeNavigation { get; set; }


    }
}
