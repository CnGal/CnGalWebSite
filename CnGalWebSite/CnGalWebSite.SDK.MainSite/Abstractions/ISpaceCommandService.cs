using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.SpaceEdit;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ISpaceCommandService
{
    /// <summary>
    /// 获取当前用户的编辑数据（个人资料 + 个人主页）。
    /// </summary>
    Task<SdkResult<SpaceEditViewModel>> GetSpaceEditAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交个人资料和个人主页的编辑。
    /// </summary>
    Task<SdkResult<string>> SubmitEditAsync(SpaceEditViewModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新当前用户的 Steam 游玩记录。
    /// </summary>
    Task<SdkResult<string>> RefreshSteamInfoAsync(string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将当前用户所有消息标记为已读。
    /// </summary>
    Task<SdkResult<string>> ReadAllMessagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行每日签到。
    /// </summary>
    Task<SdkResult<string>> SignInAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 上报用户在线状态（心跳），建议每 10 分钟调用一次。
    /// </summary>
    Task<SdkResult<string>> MakeUserOnlineAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前用户的收货地址。
    /// </summary>
    Task<SdkResult<EditUserAddressModel>> GetUserAddressAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存当前用户的收货地址。
    /// </summary>
    Task<SdkResult<bool>> EditUserAddressAsync(EditUserAddressModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取绑定群聊 QQ 的身份识别码（需要先通过人机验证）。
    /// </summary>
    Task<SdkResult<string>> GetBindGroupQQCodeAsync(UnBindGroupQQModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解除群聊 QQ 号绑定（需要先通过人机验证）。
    /// </summary>
    Task<SdkResult<string>> UnBindGroupQQAsync(UnBindGroupQQModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交用户认证申请（或取消认证）。
    /// </summary>
    Task<SdkResult<string>> EditUserCertificationAsync(EditUserCertificationModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有未被认证的词条名称列表（按类型筛选）。
    /// </summary>
    Task<SdkResult<List<string>>> GetAllNotCertificatedEntriesAsync(EntryType type, CancellationToken cancellationToken = default);
}
