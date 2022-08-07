using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Ranks
{
    public class AddUserRankModel
    {
        [Display(Name ="头衔Id")]
        public long RankId { get; set; }
        [Display(Name = "用户Id")]
        public string UserId { get; set; }
    }
}
