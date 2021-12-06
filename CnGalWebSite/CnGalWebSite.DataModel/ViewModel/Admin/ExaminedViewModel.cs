using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ExaminedViewModel
    {
        public long id { get; set; }

        public string editOverview { get; set; }

        public Operation operation { get; set; }

        public bool isPassed { get; set; }

        public string PassedAdminName { get; set; }

        public DateTime applyTime { get; set; }

        [Display(Name = "批注")]
        public string comments { get; set; }

        public string note { get; set; }

        public string applicationUserId { get; set; }

        public string applicationUserName { get; set; }

        public long entryId { get; set; }

        public string entryName { get; set; }

        public string beforeText { get; set; }

        public string afterText { get; set; }

        public dynamic beforeModel { get; set; }

        public dynamic afterModel { get; set; }


        public bool isContinued { get; set; }

        public DateTime passedTime { get; set; }

        public bool isExamined { get; set; }

        public int prepositionExamineId { get; set; }

        public string type { get; set; }
        [Display(Name = "额外积分")]
        public int Integral { get; set; }
        [Display(Name = "附加贡献值")]
        public int ContributionValue { get; set; }

        public List<string> SensitiveWords { get; set; }
    }
}
