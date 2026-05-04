namespace CnGalWebSite.SDK.MainSite.Abstractions;

/// <summary>
/// 提供当前用户标识的抽象访问器。
/// SDK.MainSite 纯核心层定义此接口，避免直接依赖 ASP.NET 的 IHttpContextAccessor。
/// ASP.NET 宿主项目通过 SDK.MainSite.AspNet 提供实现。
/// 控制台/非 Web 场景可提供空实现（始终返回 null），缓存将不按用户隔离清除。
/// </summary>
public interface ICurrentUserAccessor
{
    string? GetCurrentUserId();
}
