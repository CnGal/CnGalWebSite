using CnGalWebSite.Core.Models;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace CnGalWebSite.Core.Helpers
{
    public static class SystemEnvironmentHelper
    {
        /// <summary>
        /// 获取服务器动态数据概览
        /// </summary>
        /// <returns></returns>
        public static ServerRealTimeOverviewModel GetServerRealTimeDataOverview()
        {
            var begin = DateTime.Now;

            ServerRealTimeOverviewModel model = new ServerRealTimeOverviewModel();

            using var proc = Process.GetCurrentProcess();

            model.CPUUtilization = GetCpuUsageForProcess();
            if (double.IsNaN(model.CPUUtilization) || double.IsInfinity(model.CPUUtilization) || model.CPUUtilization < 0)
            {
                model.CPUUtilization = 0;
            }
            model.Memory = MemoryMetricsClient.GetMetrics();
            model.CPUCoreNumber = Environment.ProcessorCount;
            model.TotalProcessorTime = proc.TotalProcessorTime;
            model.ProcessUptime = DateTime.Now - proc.StartTime;

            var end = DateTime.Now;

            model.TimeSpanGetData = end - begin;

            return model;

        }
        /// <summary>
        /// 获取服务器静态数据概览
        /// </summary>
        /// <returns></returns>
        public static ServerStaticOverviewModel GetServerStaticDataOverview()
        {
            var model = new ServerStaticOverviewModel();
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            model.LastUpdateTime = File.GetLastWriteTime(assembly.Location);
            model.SystemName = Environment.OSVersion.ToString();
            model.SDKVersion = RuntimeInformation.FrameworkDescription;
            model.ServerName = Environment.MachineName;

            return model;

        }

        #region CPU 使用率

        private static DateTime? _previousCpuStartTime;
        private static TimeSpan? _previousTotalProcessorTime;

        /// <summary>
        /// 获取当前进程 CPU 使用率（百分比）
        /// 首次调用返回 0 以避免除零错误
        /// </summary>
        private static double GetCpuUsageForProcess()
        {
            var currentCpuStartTime = DateTime.UtcNow;
            TimeSpan currentCpuUsage;
            try
            {
                using var cpuProc = Process.GetCurrentProcess();
                currentCpuUsage = cpuProc.TotalProcessorTime;
            }
            catch
            {
                return 0;
            }

            // 首次调用：记录基线，返回 0
            if (!_previousCpuStartTime.HasValue)
            {
                _previousCpuStartTime = currentCpuStartTime;
                _previousTotalProcessorTime = currentCpuUsage;
                return 0;
            }

            var totalMsPassed = (currentCpuStartTime - _previousCpuStartTime.Value).TotalMilliseconds;

            // 防止间隔太短导致不准确
            if (totalMsPassed <= 0)
            {
                return 0;
            }

            var cpuUsedMs = (currentCpuUsage - _previousTotalProcessorTime.Value).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            // 记录本次数据供下次使用
            _previousCpuStartTime = currentCpuStartTime;
            _previousTotalProcessorTime = currentCpuUsage;

            return cpuUsageTotal * 100.0;
        }

        #endregion
    }

    /// <summary>
    /// 跨平台内存指标采集
    /// Windows: 使用 GC.GetGCMemoryInfo() 获取系统总物理内存
    /// Linux/Docker: 优先读取 cgroup 限制，回退到 /proc/meminfo
    /// </summary>
    public static class MemoryMetricsClient
    {
        public static MemoryMetrics GetMetrics()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return GetLinuxMetrics();
                }
                else
                {
                    return GetWindowsMetrics();
                }
            }
            catch
            {
                // Fallback: 使用进程级内存信息
                return GetProcessFallbackMetrics();
            }
        }

        /// <summary>
        /// Windows: 通过 GC API 获取系统总可用内存
        /// </summary>
        private static MemoryMetrics GetWindowsMetrics()
        {
            var metrics = new MemoryMetrics();

            // GC.GetGCMemoryInfo() 提供操作系统级别的总可用物理内存
            var gcMemInfo = GC.GetGCMemoryInfo();
            metrics.Total = gcMemInfo.TotalAvailableMemoryBytes / 1024.0 / 1024.0;

            // 使用当前进程的 WorkingSet 作为"本进程已用内存"
            using var proc = Process.GetCurrentProcess();
            metrics.Used = proc.WorkingSet64 / 1024.0 / 1024.0;
            metrics.Free = metrics.Total - metrics.Used;

            if (metrics.Free < 0)
            {
                metrics.Free = 0;
            }

            return metrics;
        }

        /// <summary>
        /// Linux/Docker: 优先使用 cgroup 限制，回退到 /proc/meminfo
        /// </summary>
        private static MemoryMetrics GetLinuxMetrics()
        {
            // 尝试 cgroup v2
            var cgroupMetrics = TryGetCgroupV2Metrics();
            if (cgroupMetrics != null)
            {
                return cgroupMetrics;
            }

            // 尝试 cgroup v1
            cgroupMetrics = TryGetCgroupV1Metrics();
            if (cgroupMetrics != null)
            {
                return cgroupMetrics;
            }

            // 回退到 /proc/meminfo
            return GetProcMeminfoMetrics();
        }

        /// <summary>
        /// cgroup v2: /sys/fs/cgroup/memory.max 和 /sys/fs/cgroup/memory.current
        /// </summary>
        private static MemoryMetrics TryGetCgroupV2Metrics()
        {
            try
            {
                const string memMaxPath = "/sys/fs/cgroup/memory.max";
                const string memCurrentPath = "/sys/fs/cgroup/memory.current";

                if (!File.Exists(memMaxPath) || !File.Exists(memCurrentPath))
                    return null;

                var maxContent = File.ReadAllText(memMaxPath).Trim();

                // "max" 表示没有限制，回退到其他方式
                if (maxContent == "max")
                    return null;

                if (!long.TryParse(maxContent, out var totalBytes))
                    return null;

                var currentContent = File.ReadAllText(memCurrentPath).Trim();
                if (!long.TryParse(currentContent, out var usedBytes))
                    return null;

                var metrics = new MemoryMetrics();
                metrics.Total = totalBytes / 1024.0 / 1024.0;
                metrics.Used = usedBytes / 1024.0 / 1024.0;
                metrics.Free = metrics.Total - metrics.Used;

                if (metrics.Free < 0)
                    metrics.Free = 0;

                return metrics;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// cgroup v1: /sys/fs/cgroup/memory/memory.limit_in_bytes 和 memory.usage_in_bytes
        /// </summary>
        private static MemoryMetrics TryGetCgroupV1Metrics()
        {
            try
            {
                const string limitPath = "/sys/fs/cgroup/memory/memory.limit_in_bytes";
                const string usagePath = "/sys/fs/cgroup/memory/memory.usage_in_bytes";

                if (!File.Exists(limitPath) || !File.Exists(usagePath))
                    return null;

                if (!long.TryParse(File.ReadAllText(limitPath).Trim(), out var limitBytes))
                    return null;

                // cgroup v1 中没有限制时通常返回一个极大值 (接近 long.MaxValue)
                // 如果超过 1TB 认为没有限制
                if (limitBytes > 1L * 1024 * 1024 * 1024 * 1024)
                    return null;

                if (!long.TryParse(File.ReadAllText(usagePath).Trim(), out var usageBytes))
                    return null;

                var metrics = new MemoryMetrics();
                metrics.Total = limitBytes / 1024.0 / 1024.0;
                metrics.Used = usageBytes / 1024.0 / 1024.0;
                metrics.Free = metrics.Total - metrics.Used;

                if (metrics.Free < 0)
                    metrics.Free = 0;

                return metrics;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 读取 /proc/meminfo 获取系统级内存信息
        /// </summary>
        private static MemoryMetrics GetProcMeminfoMetrics()
        {
            var metrics = new MemoryMetrics();
            var lines = File.ReadAllLines("/proc/meminfo");

            long totalKb = 0, availableKb = 0;

            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal:"))
                {
                    totalKb = ParseMeminfoValueKb(line);
                }
                else if (line.StartsWith("MemAvailable:"))
                {
                    availableKb = ParseMeminfoValueKb(line);
                }
            }

            metrics.Total = totalKb / 1024.0;
            metrics.Free = availableKb / 1024.0;
            metrics.Used = metrics.Total - metrics.Free;

            if (metrics.Used < 0)
                metrics.Used = 0;

            return metrics;
        }

        /// <summary>
        /// 解析 /proc/meminfo 中的一行，例如 "MemTotal:       16384000 kB"
        /// </summary>
        private static long ParseMeminfoValueKb(string line)
        {
            var parts = line.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
                return 0;

            var valuePart = parts[1].Replace("kB", "", StringComparison.OrdinalIgnoreCase).Trim();
            return long.TryParse(valuePart, out var value) ? value : 0;
        }

        /// <summary>
        /// 最终回退：使用进程级内存信息
        /// </summary>
        private static MemoryMetrics GetProcessFallbackMetrics()
        {
            var metrics = new MemoryMetrics();
            using var proc = Process.GetCurrentProcess();

            // GC 管理的内存作为已用内存的近似值
            var gcMemInfo = GC.GetGCMemoryInfo();
            metrics.Total = gcMemInfo.TotalAvailableMemoryBytes / 1024.0 / 1024.0;
            metrics.Used = proc.WorkingSet64 / 1024.0 / 1024.0;
            metrics.Free = metrics.Total - metrics.Used;

            if (metrics.Free < 0)
                metrics.Free = 0;

            return metrics;
        }
    }
}
