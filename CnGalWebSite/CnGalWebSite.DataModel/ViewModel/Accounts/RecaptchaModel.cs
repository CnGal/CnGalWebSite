using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class RecaptchaPostModel
    {
        [Display(Name = "secret")]
        public string secret { get; set; }
        [Display(Name = "response")]
        public string response { get; set; }
        [Display(Name = "remoteip")]
        public string remoteip { get; set; }
    }
    public class RecaptchaResponseModel
    {
        public bool success { get; set; }
        public string challenge_ts { get; set; }
        public string hostname { get; set; }
    }
}
