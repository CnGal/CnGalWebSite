using CnGalWebSite.IdentityServer.Models.DataModels.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Records
{
    public class OperationRecordOverviewModel
    {
        public long Id { get; set; }

        public OperationRecordType Type { get; set; }

        public string Ip { get; set; }

        public DateTime Time { get; set; }

        public string UserName { get; set; }

    }
}
