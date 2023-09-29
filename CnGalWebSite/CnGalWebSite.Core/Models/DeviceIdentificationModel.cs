using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Core.Models
{
    public class DeviceIdentificationModel
    {
        public string Ip { get; set; }

        public string Cookie { get; set; }
    }

    public class ConnectionInfo
    {
        public string RemoteIpAddress { get; set; } = "-none-";
    }
}
