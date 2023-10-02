using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ExamineViewModel
    {
        public long Id { get; set; }

        public long ObjectId { get; set; }

        public string ObjectName { get; set; }

        public string ObjectBriefIntroduction { get; set; }

        public string Image { get; set; }

        public bool IsThumbnail { get; set; }

        public ExaminedNormalListModelType Type { get; set; }

        public Operation Operation { get; set; }

        public string EditOverview { get; set; }

        public bool? IsPassed { get; set; }

        public string PassedAdminName { get; set; }

        public bool IsAdmin { get; set; }

        [Display(Name = "批注")]
        public string Comments { get; set; }

        public string Note { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public ExaminePreDataModel BeforeModel { get; set; } = new ExaminePreDataModel();

        public ExaminePreDataModel AfterModel { get; set; } = new ExaminePreDataModel();

        public DateTime ApplyTime { get; set; }

        public DateTime? PassedTime { get; set; }

        public long PrepositionExamineId { get; set; }

        [Display(Name = "附加贡献值")]
        public int ContributionValue { get; set; }

        public List<string> SensitiveWords { get; set; } = new List<string>() { };
    }

    public class ExaminePreDataModel
    {
        public List<PicturesAloneViewModel> Pictures { get; set; } = new List<PicturesAloneViewModel>();

        public List<SearchAloneModel> Relevances { get; set; } = new List<SearchAloneModel>();

        public List<InformationsModel> Texts { get; set; } = new List<InformationsModel>();

        public List<RelevancesKeyValueModel> Outlinks { get; set; } = new List<RelevancesKeyValueModel>();

        public List<EditAudioAloneModel> Audio { get; set; } = new List<EditAudioAloneModel>();

        public string MainPage { get; set; }
    }
}
