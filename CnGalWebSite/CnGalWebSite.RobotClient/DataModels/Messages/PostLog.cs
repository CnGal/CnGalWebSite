using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient.DataModels.Messages
{
    public class PostLog
    {
        public long QQ { get; set; }

        public string Message { get; set; }

        public string Reply { get; set; }

        public DateTime PostTime { get; set; }
    }
}
