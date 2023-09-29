using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Core.Models
{
    public class ServerRealTimeOverviewModel
    {
        public double CPUUtilization { get; set; }
        public long CPUFrequency { get; set; }
        public long CPUCoreNumber { get; set; }

        public TimeSpan TotalProcessorTime { get; set; }

        public TimeSpan TimeSpanGetData { get; set; }

        public MemoryMetrics Memory { get; set; } = new MemoryMetrics();

        public long NetworkBandwidth { get; set; }
    }

    public class ServerStaticOverviewModel
    {
        public string SystemName { get; set; }
        public string ServerName { get; set; }

        public string SDKVersion { get; set; }

        public DateTime LastUpdateTime { get; set; }


    }

    public class MemoryMetrics
    {
        public double Total { get; set; }
        public double Used { get; set; }
        public double Free { get; set; }

        public string TotalString { get { return ToString(Total); } }
        public string UsedString { get { return ToString(Used); } }
        public string FreeString { get { return ToString(Free); } }

        public static string ToString(double mem)
        {
            return mem > 10240 ? $"{mem / 1024:0.00} GB" : $"{mem:0.} MB";
        }
    }

}
