using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.OperationRecords
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
