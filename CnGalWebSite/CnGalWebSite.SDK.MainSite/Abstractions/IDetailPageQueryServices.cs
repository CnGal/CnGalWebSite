using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IArticlePageQueryService { Task<SdkResult<ArticlePageModel>> GetAsync(long id, CancellationToken cancellationToken = default); }
public interface IVideoPageQueryService { Task<SdkResult<VideoPageModel>> GetAsync(long id, CancellationToken cancellationToken = default); }
public interface ITagPageQueryService { Task<SdkResult<TagPageModel>> GetAsync(int id, CancellationToken cancellationToken = default); }
public interface IPeripheryPageQueryService { Task<SdkResult<PeripheryPageModel>> GetAsync(long id, CancellationToken cancellationToken = default); }
