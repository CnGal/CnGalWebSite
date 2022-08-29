using CnGalWebSite.DataModel.ViewModel.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.EditRecords
{
    public class UserUnReadEditRecordViewModel
    {
        /// <summary>
        /// 用户监视的词条的未读审核记录
        /// </summary>
        public List<ExaminedNormalListModel> UnreadExamines { get; set; } = new List<ExaminedNormalListModel>();
        /// <summary>
        /// 待审核的编辑记录
        /// </summary>
        public List<ExaminedNormalListModel> PendingExamines { get; set; } = new List<ExaminedNormalListModel>();

    }
}
