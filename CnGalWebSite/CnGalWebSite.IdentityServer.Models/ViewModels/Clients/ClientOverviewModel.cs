using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Clients
{
    public class ClientOverviewModel
    {
        public int Id { get; set; }

        public bool Enabled { get; set; } = true;

        public string ClientId { get; set; }

        public string ClientName { get; set; }

        public string ClientUri { get; set; }

        public string LogoUri { get; set; }

        public string Description { get; set; }

        public bool RequirePkce { get; set; } = true;

        public bool AllowAccessTokensViaBrowser { get; set; }

        public bool AllowOfflineAccess { get; set; } = true;

        public int AccessTokenLifetime { get; set; } = 3600;

        public int AbsoluteRefreshTokenLifetime { get; set; } = 2592000;

        public int SlidingRefreshTokenLifetime { get; set; } = 1296000;

        public ClientTokenUsage RefreshTokenUsage { get; set; } = ClientTokenUsage.OneTimeOnly;

        public ClientTokenExpiration RefreshTokenExpiration { get; set; } = ClientTokenExpiration.Absolute;
    }

    public enum ClientTokenUsage
    {
        /// <summary>
        /// Re-use the refresh token handle
        /// </summary>
        [Display(Name = "可重复使用")]
        ReUse = 0,

        /// <summary>
        /// Issue a new refresh token handle every time
        /// </summary>
        [Display(Name = "一次性")]
        OneTimeOnly = 1
    }

    /// <summary>
    /// Token expiration types.
    /// </summary>
    public enum ClientTokenExpiration
    {
        /// <summary>
        /// Sliding token expiration
        /// </summary>
        [Display(Name = "滑动")]
        Sliding = 0,

        /// <summary>
        /// Absolute token expiration
        /// </summary>
        [Display(Name = "固定")]
        Absolute = 1
    }

}
