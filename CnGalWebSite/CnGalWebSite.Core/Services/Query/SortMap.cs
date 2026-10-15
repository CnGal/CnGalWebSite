using System.Linq.Expressions;
using CnGalWebSite.Core.Models;

namespace CnGalWebSite.Core.Services.Query;

/// <summary>Compiler-checked selectors preserve each key's actual type for query providers.</summary>
public sealed class SortMap<TModel, TSort> where TSort : struct, Enum
{
    private readonly Dictionary<TSort, ISortKey> _keys = new();
    private readonly SortOption<TSort> _default;
    private readonly TSort _tieBreaker;

    public SortMap(TSort defaultField, TSort tieBreaker, SortDirection direction = SortDirection.Descending)
    {
        _default = new(defaultField, direction);
        _tieBreaker = tieBreaker;
    }

    public SortMap<TModel, TSort> Add<TKey>(TSort field, Expression<Func<TModel, TKey>> selector)
    {
        _keys.Add(field, new SortKey<TKey>(selector));
        return this;
    }

    public IOrderedQueryable<TModel> Apply(IQueryable<TModel> query, IReadOnlyList<SortOption<TSort>> sorts)
    {
        if (sorts == null) throw new ArgumentException("排序不能为空", nameof(sorts));
        IReadOnlyList<SortOption<TSort>> effective = sorts.Count == 0 ? new[] { _default } : sorts;
        foreach (var sort in effective)
            if (sort == null || !Enum.IsDefined(sort.Field) || !Enum.IsDefined(sort.Direction) || !_keys.ContainsKey(sort.Field))
                throw new ArgumentException("排序字段或方向无效", nameof(sorts));

        IOrderedQueryable<TModel> ordered = null;
        foreach (var sort in effective)
            ordered = _keys[sort.Field].Apply(query, ordered, sort.Direction);
        if (!effective.Any(sort => EqualityComparer<TSort>.Default.Equals(sort.Field, _tieBreaker)))
            ordered = _keys[_tieBreaker].Apply(query, ordered, SortDirection.Ascending);
        return ordered;
    }

    private interface ISortKey
    {
        IOrderedQueryable<TModel> Apply(IQueryable<TModel> query, IOrderedQueryable<TModel> ordered, SortDirection direction);
    }

    private sealed class SortKey<TKey>(Expression<Func<TModel, TKey>> selector) : ISortKey
    {
        public IOrderedQueryable<TModel> Apply(IQueryable<TModel> query, IOrderedQueryable<TModel> ordered, SortDirection direction)
            => ordered == null
                ? direction == SortDirection.Descending ? query.OrderByDescending(selector) : query.OrderBy(selector)
                : direction == SortDirection.Descending ? ordered.ThenByDescending(selector) : ordered.ThenBy(selector);
    }
}
