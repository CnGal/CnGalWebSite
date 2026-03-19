using CnGalWebSite.DataModel.ViewModel.Space;

namespace CnGalWebSite.SDK.MainSite.Models.SpaceEdit;

/// <summary>
/// 用户编辑页聚合 ViewModel，包含个人资料 + 个人主页两部分数据。
/// </summary>
public sealed class SpaceEditViewModel
{
    /// <summary>
    /// 个人资料编辑数据（用户名、头像、签名、隐私设置等）。
    /// </summary>
    public EditUserDataViewModel UserData { get; set; } = new();

    /// <summary>
    /// 个人主页编辑数据（Markdown 正文）。
    /// </summary>
    public EditUserMainPageViewModel MainPage { get; set; } = new();
}
