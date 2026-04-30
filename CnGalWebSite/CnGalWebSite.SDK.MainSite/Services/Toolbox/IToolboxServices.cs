using CnGalWebSite.SDK.MainSite.Models.Toolbox;

namespace CnGalWebSite.SDK.MainSite.Services.Toolbox;

public interface IToolboxProgressService
{
    event Action<ToolboxProgressUpdate>? ProgressUpdate;
}

public interface IToolboxVideoRepostService : IToolboxProgressService
{
    Task ProcessVideoAsync(RepostVideoTaskModel model, CancellationToken cancellationToken = default);
}

public interface IToolboxArticleRepostService : IToolboxProgressService
{
    Task ProcessArticleAsync(RepostArticleTaskModel model, CancellationToken cancellationToken = default);
}

public interface IToolboxEntryMergeService : IToolboxProgressService
{
    Task ProcessMergeEntryAsync(MergeEntryTaskModel model, CancellationToken cancellationToken = default);
}
