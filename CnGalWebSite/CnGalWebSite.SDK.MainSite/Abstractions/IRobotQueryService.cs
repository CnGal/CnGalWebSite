using CnGalWebSite.DataModel.ViewModel.Robots;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IRobotQueryService
{
    Task<RobotQueryResult> SearchAsync(string text, int currentPage = 1, CancellationToken cancellationToken = default);
    Task<RobotQueryResult> GetEntryAsync(int id, CancellationToken cancellationToken = default);
    Task<RobotQueryResult> GetArticleAsync(long id, CancellationToken cancellationToken = default);
    Task<RobotQueryResult> GetLatestGamesAsync(CancellationToken cancellationToken = default);
    Task<RobotQueryResult> GetLatestDemoGamesAsync(CancellationToken cancellationToken = default);
    Task<RobotQueryResult> GetUpcomingGamesAsync(CancellationToken cancellationToken = default);
    Task<RobotQueryResult> GetNewsAsync(CancellationToken cancellationToken = default);
    Task<RobotQueryResult> GetRandomGameAsync(CancellationToken cancellationToken = default);
    Task<RobotQueryResult> GetRoleBirthdaysAsync(int? month = null, int? day = null, CancellationToken cancellationToken = default);
    Task<RobotQueryResult> GetRecentlyUpdatedGamesAsync(CancellationToken cancellationToken = default);
    Task<RobotDiscountSummary> GetDiscountSummaryAsync(CancellationToken cancellationToken = default);
}
