using System.Linq.Expressions;
using CnGalWebSite.Core.Models;

namespace CnGalWebSite.Core.Services.Query;

public interface IQueryService
{
    Task<(IQueryable<TModel>, int)> QueryAsync<TModel, TSort>(IQueryable<TModel> query,
        QueryParameterModel<TSort> model, Expression<Func<TModel, bool>> predicate,
        SortMap<TModel, TSort> sorting) where TModel : class where TSort : struct, Enum;
}
