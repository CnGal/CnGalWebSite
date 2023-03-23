using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Clients;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.DataModels.Examines
{
    public class Examine
    {
        public long Id { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public bool? IsPassed { get; set; }

        public DateTime? PassedTime { get; set; }

        public DateTime ApplyTime { get; set; }

        public string PassedAdminName { get; set; }

        public ExamineType Type { get; set; }

        public long? UserClientId { get; set; }
        public UserClient UserClient { get; set; }

    }

    public enum ExamineType
    {
        [Display(Name ="客户端")]
        Client
    }
}
