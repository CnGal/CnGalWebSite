using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.News
{
    public class AddWeiboNewsModel
    {
        [Display(Name = "微博链接")]
        public string Link { get; set; }
    }
}
