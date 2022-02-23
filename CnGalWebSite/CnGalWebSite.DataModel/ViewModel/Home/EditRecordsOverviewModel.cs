using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class ExaminesOverviewViewModel
    {
        public long ObjectId { get; set; }

        public string ObjectName { get; set; }

        public string ObjectBriefIntroduction { get; set; }


        public string Image { get; set; }

        public bool IsThumbnail { get; set; }

        public ExaminedNormalListModelType Type { get; set; }

        public List<EditRecordAloneViewModel> Examines { get; set; } = new List<EditRecordAloneViewModel>();
    }
    public class EditRecordAloneViewModel
    {
        public long Id { get; set; }

        public bool IsSelected { get; set; }

        public string EditOverview { get; set; }

        public Operation Operation { get; set; }

        public string PassedAdminName { get; set; }

        public DateTime ApplyTime { get; set; }

        public string Comments { get; set; }

        public string Note { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public DateTime PassedTime { get; set; }
    }
}
