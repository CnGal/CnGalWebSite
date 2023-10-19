using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.OperationRecords
{
    public class OperationRecordOverviewModel
    {
        public long Id { get; set; }

        public OperationRecordType Type { get; set; }

        public PageType PageType { get; set; }

        public long PageId { get; set; }

        public string IP { get; set; }

        public string Cookie { get; set; }

        public DateTime Time { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }    
    }
}
