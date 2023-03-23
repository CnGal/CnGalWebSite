using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.DataModels.Clients
{
    public class UserClient
    {
        public long Id { get; set; }

        public string ClientName { get; set; }

        public string ClientUri { get; set; }

        public string LogoUri { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// 关联客户端的数字Id
        /// </summary>
        public long ClientId { get; set; }

        public bool? IsPassed { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Examine> Examines { get; set; } = new List<Examine>();

    }
}
