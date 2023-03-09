using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.HealthCheck.Models
{
    public class ServiceStatus
    {
        public string Service { get; set; }
        public HealthStatus Status { get; set; }
        public List<KeyValuePair<string,object>> Data { get; set; }

        public static  HealthCheckOptions Options = new()
        {
            ResponseWriter = async (c, r) => {

                c.Response.ContentType = "application/json";
                var result = new List<ServiceStatus>
                {
                    new ServiceStatus { Service = "OverAll", Status = r.Status }
                };
                result.AddRange(
                    r.Entries.Select(
                        e => new ServiceStatus
                        {
                            Service = e.Key,
                            Status = e.Value.Status,
                            Data = e.Value.Data.Select(k => k).ToList()
                        }
                    )
                );

                var json = JsonSerializer.Serialize(result);

                await c.Response.WriteAsync(json);
            }
        };
    }
}
