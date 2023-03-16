using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.Geetest
{
    public class GeetestCodeModel
    {
        public string Gt { get; set; }
        public string Challenge { get; set; }
        public string Success { get; set; }
    }
}
