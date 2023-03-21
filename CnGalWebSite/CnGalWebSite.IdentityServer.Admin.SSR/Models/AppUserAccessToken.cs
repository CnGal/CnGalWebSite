using IdentityModel.AspNetCore.AccessTokenManagement;

namespace CnGalWebSite.IdentityServer.Admin.SSR.Models
{
    public class AppUserAccessToken
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public string RefreshToken { get; set; }

    }
}
