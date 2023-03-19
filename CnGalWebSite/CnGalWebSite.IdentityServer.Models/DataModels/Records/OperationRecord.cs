using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CnGalWebSite.IdentityServer.Models.DataModels.Records
{
    public class OperationRecord
    {
        public long Id { get; set; }

        public OperationRecordType Type { get; set; }

        public string Ip { get; set; }

        public DateTime Time { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }

    public enum OperationRecordType
    {
        [Display(Name = "登入")]
        Login,
        [Display(Name = "注册")]
        Registe,
    }
}
