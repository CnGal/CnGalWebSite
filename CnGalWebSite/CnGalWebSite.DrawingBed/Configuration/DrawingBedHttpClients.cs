using Microsoft.Security.AntiSSRF;
using System.Net;

namespace CnGalWebSite.DrawingBed.Configuration;

public static class DrawingBedHttpClients
{
    public static IServiceCollection AddDrawingBedHttpClients(this IServiceCollection services)
    {
        // 图床固定直连；AntiSSRF 1.0.0 未公开底层 handler 的代理设置。
        HttpClient.DefaultProxy = new WebProxy();
        services.AddHttpClient("safeOutbound")
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var policy = new AntiSSRFPolicy(PolicyConfigOptions.ExternalOnlyLatest)
                {
                    AllowPlainTextHttp = true
                };
                var handler = policy.GetHandler();
                // AntiSSRF 1.0.0 会跟随 HTTPS 降级，下载路径自行逐跳检查。
                handler.AllowAutoRedirect = false;
                handler.ConnectTimeout = TimeSpan.FromSeconds(10);
                handler.UseCookies = false;
                return handler;
            });
        return services;
    }
}
