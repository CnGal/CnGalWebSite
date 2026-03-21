using CnGalWebSite.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 设备识别服务实现（InteractiveServer 模式下运行）。
/// 通过 IHttpContextAccessor 获取 IP/UA，通过 ProtectedSessionStorage 持久化 Cookie ID。
/// </summary>
public sealed class CgDeviceIdentificationService(
    IHttpContextAccessor httpContextAccessor,
    ProtectedSessionStorage sessionStorage) : ICgDeviceIdentificationService
{
    private const string CookieStorageKey = "cg-device-identification-cookie";

    public async Task<DeviceIdentificationModel> GetIdentificationAsync()
    {
        var model = new DeviceIdentificationModel();

        // 获取 IP
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            model.Ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            model.UA = httpContext.Request.Headers.UserAgent.ToString();
        }

        // 获取或生成持久化 Cookie
        try
        {
            var cookieResult = await sessionStorage.GetAsync<string>(CookieStorageKey);
            if (cookieResult.Success && !string.IsNullOrWhiteSpace(cookieResult.Value))
            {
                model.Cookie = cookieResult.Value;
            }
            else
            {
                model.Cookie = Guid.NewGuid().ToString();
                await sessionStorage.SetAsync(CookieStorageKey, model.Cookie);
            }
        }
        catch
        {
            // ProtectedSessionStorage 可能在预渲染阶段抛异常，兜底生成一个临时 Cookie
            model.Cookie = Guid.NewGuid().ToString();
        }

        return model;
    }
}
