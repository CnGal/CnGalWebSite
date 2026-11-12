using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Disambigs;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.Application.Tags;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Videos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;

namespace CnGalWebSite.APIServer.Application.Examines;

/// <summary>
/// Historical-preview adapter for the same application services used by the approved-write
/// flow. It owns dispatch only; field and collection semantics remain in the domain services.
/// </summary>
public interface IExaminePatchApplicationService
{
    /// <summary>Create an empty aggregate used for deterministic history replay.</summary>
    object CreateTarget(ExaminedNormalListModelType type);

    /// <summary>Apply one persisted examine record to a detached or tracked target.</summary>
    Task ApplyAsync(object target, Examine examine);
}

public sealed class ExaminePatchApplicationService(
    IEntryService entryService,
    IArticleService articleService,
    ITagService tagService,
    IPeripheryService peripheryService,
    IVideoService videoService,
    IUserService userService,
    IPlayedGameService playedGameService,
    IFavoriteFolderService favoriteFolderService,
    IDisambigService disambigService) : IExaminePatchApplicationService
{
    public object CreateTarget(ExaminedNormalListModelType type) => CreateEmptyTarget(type);

    public static object CreateEmptyTarget(ExaminedNormalListModelType type) => type switch
    {
        ExaminedNormalListModelType.Entry => new Entry(),
        ExaminedNormalListModelType.Article => new Article(),
        ExaminedNormalListModelType.Tag => new Tag(),
        ExaminedNormalListModelType.Periphery => new Periphery(),
        ExaminedNormalListModelType.Video => new Video(),
        ExaminedNormalListModelType.User => new ApplicationUser(),
        ExaminedNormalListModelType.PlayedGame => new PlayedGame(),
        ExaminedNormalListModelType.FavoriteFolder => new FavoriteFolder(),
        ExaminedNormalListModelType.Disambig => new Disambig
        {
            Articles = new List<Article>(),
            Entries = new List<Entry>(),
            Examines = new List<Examine>()
        },
        _ => throw new InvalidOperationException($"不支持的历史预览类型：{type}")
    };

    public async Task ApplyAsync(object target, Examine examine)
    {
        switch (target)
        {
            case Entry entry when IsEntryOperation(examine.Operation):
                await entryService.UpdateEntryDataAsync(entry, examine);
                return;
            case Article article when IsArticleOperation(examine.Operation):
                await articleService.UpdateArticleData(article, examine);
                return;
            case Tag tag when IsTagOperation(examine.Operation):
                await tagService.UpdateTagDataAsync(tag, examine);
                return;
            case Periphery periphery when IsPeripheryOperation(examine.Operation):
                await peripheryService.UpdatePeripheryDataAsync(periphery, examine);
                return;
            case Video video when IsVideoOperation(examine.Operation):
                await videoService.UpdateData(video, examine);
                return;
            case ApplicationUser user when IsUserOperation(examine.Operation):
                await userService.UpdateUserData(user, examine);
                return;
            case PlayedGame playedGame when examine.Operation is Operation.EditPlayedGameMain:
                playedGameService.UpdatePlayedGameData(playedGame, examine);
                return;
            case FavoriteFolder favoriteFolder when examine.Operation is Operation.EditFavoriteFolderMain:
                favoriteFolderService.UpdateData(favoriteFolder, examine);
                return;
            case Disambig disambig when examine.Operation is Operation.DisambigMain or Operation.DisambigRelevances:
                await disambigService.UpdateDisambigDataAsync(disambig, examine);
                return;
            default:
                throw new InvalidOperationException(
                    $"审核操作 {examine.Operation} 不能应用到目标类型 {target.GetType().Name}。");
        }
    }

    private static bool IsEntryOperation(Operation operation) => operation is
        Operation.EstablishMain or Operation.EstablishAddInfor or Operation.EstablishMainPage or
        Operation.EstablishImages or Operation.EstablishRelevances or Operation.EstablishTags or
        Operation.EstablishAudio or Operation.EstablishWebsite;

    private static bool IsArticleOperation(Operation operation) => operation is
        Operation.EditArticleMain or Operation.EditArticleRelevanes or Operation.EditArticleMainPage;

    private static bool IsTagOperation(Operation operation) => operation is
        Operation.EditTag or Operation.EditTagMain or Operation.EditTagChildTags or Operation.EditTagChildEntries;

    private static bool IsPeripheryOperation(Operation operation) => operation is
        Operation.EditPeripheryMain or Operation.EditPeripheryImages or
        Operation.EditPeripheryRelatedEntries or Operation.EditPeripheryRelatedPeripheries;

    private static bool IsVideoOperation(Operation operation) => operation is
        Operation.EditVideoMain or Operation.EditVideoImages or Operation.EditVideoMainPage or Operation.EditVideoRelevanes;

    private static bool IsUserOperation(Operation operation) => operation is
        Operation.EditUserMain or Operation.UserMainPage;
}
