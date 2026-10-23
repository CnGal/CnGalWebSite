namespace CnGalWebSite.SDK.MainSite.Infrastructure;

/// <summary>
/// 查询和写操作共用的审核缓存键，沿用 typed-sort 命名空间。
/// 用户相关缓存还通过 ExamineUserCache 进一步隔离。
/// </summary>
internal static class ExamineCacheKeys
{
    private const string Prefix = "main-site:typed-sort-v1:";

    public const string ContentCenter = Prefix + "user-content-center";
    public static string List<TSort>(string apiPath, string queryKey) where TSort : struct, Enum =>
        $"{Prefix}examine-list:{apiPath}:{typeof(TSort).FullName}:{queryKey}";

    public static string Detail(long examineId) => $"{Prefix}examine-detail:{examineId}";
    public static string Overview(long examineId) => $"{Prefix}examine-overview:{examineId}";
    public static string EntryEditRecords(long entryId) => $"{Prefix}entry-edit-records:{entryId}";
    public static string ArticleEditRecords(long articleId) => $"{Prefix}article-edit-records:{articleId}";
    public static string VideoEditRecords(long videoId) => $"{Prefix}video-edit-records:{videoId}";
    public static string TagEditRecords(long tagId) => $"{Prefix}tag-edit-records:{tagId}";
    public static string PeripheryEditRecords(long peripheryId) => $"{Prefix}periphery-edit-records:{peripheryId}";
}
