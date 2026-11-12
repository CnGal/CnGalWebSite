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

        public bool SubmissionOnly { get; set; }

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
        public List<RecordPreviewItem> Items { get; set; } = new List<RecordPreviewItem>();
        public bool IsComplete { get; set; } = true;
    }

    public enum RecordPreviewKind { Body, Text, Image, Relation, Link, Audio }

    public class RecordPreviewItem
    {
        public string Key { get; set; }
        public string Group { get; set; }
        public RecordPreviewKind Kind { get; set; }
        public string Label { get; set; }
        // Canonical value participates in comparisons; labels and resolved links do not.
        public string Value { get; set; }
        public string DisplayValue { get; set; }
        public string Url { get; set; }
        public string Html { get; set; }
        public List<RecordPreviewProperty> Properties { get; set; } = new List<RecordPreviewProperty>();
        public bool IsKnown { get; set; } = true;
    }

    public class RecordPreviewProperty
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public string DisplayValue { get; set; }
    }

    public class EditRecordComparison
    {
        public long ContrastId { get; set; }
        public long CurrentId { get; set; }
        public ExaminePreDataModel Before { get; set; } = new ExaminePreDataModel();
        public ExaminePreDataModel After { get; set; } = new ExaminePreDataModel();
    }
}
