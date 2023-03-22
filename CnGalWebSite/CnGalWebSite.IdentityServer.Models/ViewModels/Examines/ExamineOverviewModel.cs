
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Examines
{
    public class ExamineOverviewModel
    {
        public long Id { get; set; }

        public bool? IsPassed { get; set; }

        public DateTime? PassedTime { get; set; }

        public DateTime ApplyTime { get; set; }

        public string UserName { get; set; }

        public string PassedAdminName { get; set; }

        public ExamineType Type { get; set; }
    }
}
