namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class UnBindInputModel
    {
        public SelectModifyFieldType Type { get; set; }
        public string Provider { get; set; }

        public string SecondCode { get; set; }
        public string ReturnUrl { get; set; }
    }
}
