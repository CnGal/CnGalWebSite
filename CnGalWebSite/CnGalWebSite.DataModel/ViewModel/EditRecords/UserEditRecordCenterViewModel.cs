using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.EditRecords
{
    public class UserContentCenterViewModel
    {
        /// <summary>
        /// 用户监视的词条的未读审核记录
        /// </summary>
        public List<ExaminedNormalListModel> UnReviewExamines { get; set; } = new List<ExaminedNormalListModel>();
        /// <summary>
        /// 待审核的编辑记录
        /// </summary>
        public List<ExaminedNormalListModel> PendingExamines { get; set; } = new List<ExaminedNormalListModel>();
        /// <summary>
        /// 未读的词条
        /// </summary>
        public List<EntryInforTipViewModel> UnReviewEntries { get; set; } = new List<EntryInforTipViewModel>();

        public UserEditInforBindModel UserEditInfor { get; set; } = new UserEditInforBindModel();

    }
}
