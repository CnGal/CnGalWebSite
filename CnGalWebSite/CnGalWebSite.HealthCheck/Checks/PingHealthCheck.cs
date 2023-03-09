using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Threading;
using System;
using CnGalWebSite.Helper.Extensions;

namespace CnGalWebSite.HealthCheck.Checks
{
    public class PingHealthCheck : IHealthCheck
    {
        private string _host;
        private int _timeout;
        private int _pingInterval;
        private DateTime _lastPingTime = DateTime.MinValue;
        private HealthCheckResult _lastPingResult = HealthCheckResult.Healthy();
        private static object _locker = new object();


        public PingHealthCheck(string host, int timeout, int pingInterval = 0)
        {
            _host =( host.MidStrEx("//", "/")??host).Split(':').FirstOrDefault();
            _timeout = timeout;
            _pingInterval = pingInterval;
        }

        private bool IsCacheExpired()
        {
            return (_pingInterval == 0 || _lastPingTime.AddSeconds(_pingInterval) <= DateTime.Now);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!IsCacheExpired())
            {
                return await Task.FromResult(_lastPingResult);
            }


                        PingService();


            return await Task.FromResult(_lastPingResult);
        }

        private void PingService()
        {
            try
            {
                using var ping = new Ping();
                _lastPingTime = DateTime.Now;

                var reply = ping.Send(_host, _timeout);

                var status = HealthStatus.Healthy;

                if (reply.Status != IPStatus.Success)
                {
                    status = HealthStatus.Unhealthy;
                }
                else if (reply.RoundtripTime >= _timeout)
                {
                    status = HealthStatus.Degraded;
                }

                var data = new Dictionary<string, object>
                {
                    { "Roundtrip time", reply.RoundtripTime},
                };
                _lastPingResult = new HealthCheckResult(status, null, null, data);
            }
            catch(Exception ex)
            {
                _lastPingResult = HealthCheckResult.Unhealthy();
            }
        }
    }
}
