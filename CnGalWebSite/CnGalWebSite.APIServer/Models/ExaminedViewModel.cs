﻿
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.APIServer.Models
{
    public class ExaminedViewModel
    {
        public long Id { get; set; }

        public string EditOverview { get; set; }

        public Operation Operation { get; set; }

        public bool IsPassed { get; set; }

        public string PassedAdminName { get; set; }

        public DateTime ApplyTime { get; set; }
        [Display(Name = "批注")]
        public string Comments { get; set; }

        public string Note { get; set; }

        public string ApplicationUserId { get; set; }

        public string ApplicationUserName { get; set; }

        public long EntryId { get; set; }

        public string EntryName { get; set; }

        public string BeforeText { get; set; }

        public string AfterText { get; set; }

        public dynamic BeforeModel { get; set; }

        public dynamic AfterModel { get; set; }


        public bool IsContinued { get; set; }

        public DateTime PassedTime { get; set; }

        public bool IsExamined { get; set; }

        public int PrepositionExamineId { get; set; }

        public string Type { get; set; }
        [Display(Name = "额外积分")]
        public int Integral { get; set; }
        [Display(Name = "附加贡献值")]
        public int ContributionValue { get; set; }

        public List<string> SensitiveWords { get; set; } = new List<string>() { };
    }

}
