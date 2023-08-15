using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClientX.Models.Events
{
    public class EventExecuteInfor
    {
        public long Id { get; set; }

        public string Note { get; set; }

        public bool RealExecute { get; set; }

        public DateTime LastRunTime { get; set; }
    }
}
