using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class EditArticleMainPageViewModel : BaseEditModel
    {
        [Display(Name = "正文")]
        [Required(ErrorMessage = "请输入正文")]
        public string Context { get; set; }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Context))
            {
                return new Result { Error = "请输入正文" };
            }

            return new Result { Successful = true };
        }
    }
}
