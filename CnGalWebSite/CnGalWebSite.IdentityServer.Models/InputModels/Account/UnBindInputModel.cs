using CnGalWebSite.IdentityServer.Models.ViewModels.Account;

namespace CnGalWebSite.IdentityServer.Models.InputModels.Account
{
    public class UnBindInputModel
    {
        public SelectModifyFieldType Type { get; set; }
        public string Provider { get; set; }

        public string SecondCode { get; set; }
        public string ReturnUrl { get; set; }
    }
}
