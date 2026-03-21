using CnGalWebSite.Core.Models;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 设备识别服务接口（反作弊：收集 IP/Cookie/UA）。
/// </summary>
public interface ICgDeviceIdentificationService
{
    Task<DeviceIdentificationModel> GetIdentificationAsync();
}
