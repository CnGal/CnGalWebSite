using System.Linq.Expressions;
using CnGalWebSite.Core.Models;

namespace CnGalWebSite.Core.Services.Query;

public class QueryService : IQueryService
{
    public Task<(IQueryable<TModel>, int)> QueryAsync<TModel, TSort>(IQueryable<TModel> query,
        QueryParameterModel<TSort> model, Expression<Func<TModel, bool>> predicate,
        SortMap<TModel, TSort> sorting) where TModel : class where TSort : struct, Enum
    {
        var filtered = query.Where(predicate);
        IQueryable<TModel> items = sorting.Apply(filtered, model.Sorts);
        var total = filtered.Count();
        if (model.ItemsPerPage > 0)
            items = items.Skip((Math.Max(1, model.Page) - 1) * model.ItemsPerPage).Take(model.ItemsPerPage);
        return Task.FromResult((items, total));
    }
}
