using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.DataModels.Geetest
{
    public class GeetestCodeModel
    {
        public string Gt { get; set; }
        public string Challenge { get; set; }
        public string Success { get; set; }
    }
}
