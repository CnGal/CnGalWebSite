using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IPlayedGameCommandService
{
    /// <summary>
    /// 获取游玩记录编辑数据（GET api/playedgame/EditGameRecord/{gameId}）。
    /// </summary>
    Task<SdkResult<EditGameRecordModel>> GetEditDataAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存游玩记录（POST api/playedgame/EditGameRecord）。
    /// </summary>
    Task<SdkResult<bool>> SaveAsync(EditGameRecordModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 切换游玩记录隐藏状态（POST api/playedgame/HiddenGameRecord）。
    /// </summary>
    Task<SdkResult<bool>> ToggleHiddenAsync(int gameId, bool isHidden, CancellationToken cancellationToken = default);
}
