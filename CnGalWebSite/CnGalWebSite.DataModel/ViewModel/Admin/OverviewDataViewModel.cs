using System;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ServerRealTimeOverviewModel
    {
        public double CPUUtilization { get; set; }
        public long CPUFrequency { get; set; }
        public long CPUCoreNumber { get; set; }

        public TimeSpan TotalProcessorTime { get; set; }

        public TimeSpan TimeSpanGetData { get; set; }

        public MemoryMetrics Memory { get; set; }=new MemoryMetrics();

        public long NetworkBandwidth { get; set; }
    }

    public class ServerStaticOverviewModel
    {
        public string SystemName { get; set; }
        public string ServerName { get; set; }

        public string SDKVersion { get; set; }

        public DateTime LastUpdateTime  { get; set; }


    }

    public class UserDataOverviewModel
    {
        public long UserTotalNumber { get; set; }

        public long UserVerifyEmailNumber { get; set; }
        public long UserSecondaryVerificationNumber { get; set; }
        public long UserActiveNumber { get; set; }
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
