using CnGalWebSite.DataModel.ViewModel.Admin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Helper.Helper
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
            model.Memory = new MemoryMetricsClient().GetMetrics();
            model.CPUCoreNumber = Environment.ProcessorCount;
            model.TotalProcessorTime = proc.TotalProcessorTime;

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

        private static DateTime? _previousCpuStartTime = null;
        private static TimeSpan? _previousTotalProcessorTime = null;

        private static double GetCpuUsageForProcess()
        {
            var currentCpuStartTime = DateTime.UtcNow;
            var currentCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            // If no start time set then set to now
            if (!_previousCpuStartTime.HasValue)
            {
                _previousCpuStartTime = currentCpuStartTime;
                _previousTotalProcessorTime = currentCpuUsage;
            }

            var cpuUsedMs = (currentCpuUsage - _previousTotalProcessorTime.Value).TotalMilliseconds;
            var totalMsPassed = (currentCpuStartTime - _previousCpuStartTime.Value).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            // Set previous times.
            _previousCpuStartTime = currentCpuStartTime;
            _previousTotalProcessorTime = currentCpuUsage;

            return cpuUsageTotal * 100.0;
        }
    }

    public class MemoryMetricsClient
    {
        public MemoryMetrics GetMetrics()
        {
            if (IsUnix())
            {
                return GetUnixMetrics();
            }

            return GetWindowsMetrics();
        }

        private bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

        private MemoryMetrics GetWindowsMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split("\n");
            var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
            metrics.Used = metrics.Total - metrics.Free;

            return metrics;
        }

        private MemoryMetrics GetUnixMetrics()
        {
            var output = "";
            var metrics = new MemoryMetrics();

            //获取已使用内存
            var info = new ProcessStartInfo("free -m")
            {
                FileName = "/bin/bash",
                Arguments = "-c \"cat /sys/fs/cgroup/memory/memory.usage_in_bytes\"",
                RedirectStandardOutput = true
            };
            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }
            if (string.IsNullOrWhiteSpace(output) == false)
            {
                metrics.Used = long.Parse(output) / 1024.0 / 1024.0;
            }


            //获取容器内存限制
            info.Arguments = "-c \"cat /sys/fs/cgroup/memory/memory.limit_in_bytes\"";
            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }
            if (string.IsNullOrWhiteSpace(output) == false)
            {
                metrics.Total = long.Parse(output) / 1024.0 / 1024.0;
            }
             

            //获取当前系统内存上限
            info.Arguments = "-c \"free -m\"";
            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }
            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            //判断是否为空
            if(metrics.Used==0)
            {
                metrics.Used = double.Parse(memory[2]);
            }
            if (metrics.Total == 0)
            {
                metrics.Total = double.Parse(memory[1]);
            }
            //内存限制超过系统内存 则使用系统内存
            metrics.Total = metrics.Total > double.Parse(memory[1]) ? double.Parse(memory[1]) : metrics.Total;

            //计算可用内存
            metrics.Free = metrics.Total - metrics.Used;

            return metrics;
        }
    }
}
