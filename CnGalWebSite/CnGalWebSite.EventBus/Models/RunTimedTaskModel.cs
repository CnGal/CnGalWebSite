using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.EventBus.Models
{
    public class RunTimedTaskModel
    {
        public int Type { get; set; }

        public string Note { get; set; }

        public string Parameter { get; set; }

        public DateTime? LastExecutedTime { get; set; }
    }
}
