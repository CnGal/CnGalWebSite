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

        public long MemoryTotalSize { get; set; }
        public long MemoryUsedSize { get; set; }

        public long NetworkBandwidth { get; set; }
    }

    public class ServerStaticOverviewModel
    {
        public string SystemName { get; set; }
        public string ServerName { get; set; }

        public string SDKVersion { get; set; }

        public DateTime LastUpdateTime  { get; set; }

        public long UserTotalNumber { get; set; }

        public long UserVerifyEmailNumber { get; set; }
        public long UserSecondaryVerificationNumber { get; set; }
        public long UserActiveNumber { get; set; }
    }
}
