using CnGalWebSite.Core.Models;

namespace CnGalWebSite.Core.Services.Query;

public static class SortCacheKey
{
    public static string Create<TSort>(IEnumerable<SortOption<TSort>> sorts) where TSort : struct, Enum
        => typeof(TSort).FullName + ":" + string.Join(",", sorts.Select(sort => $"{sort.Field}:{sort.Direction}"));
}
