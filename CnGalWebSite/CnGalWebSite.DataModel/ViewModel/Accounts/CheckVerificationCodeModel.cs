using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class CheckVerificationCodeModel
    {
        [Display(Name = "验证码")]
        public string Num { get; set; }
    }
}
