
using System;

namespace CnGalWebSite.BlazorWeb.Models.Tokens
{
    public class AppUserAccessToken
    {
        public long Id { get; set; }
        public AppUserAccessTokenType Type { get; set; }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public string RefreshToken { get; set; }
    }

    public enum AppUserAccessTokenType
    {
        IdsAdmin,
        Server
    }
}
