
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using System;
using CnGalWebSite.APIServer.DataReositories;

namespace CnGalWebSite.IdentityServer.Services.Shared
{
    public interface IQueryService
    {
        Task<(IQueryable<TModel>, int)> QueryAsync<TModel, Tkey>(IQueryable<TModel> _query, QueryParameterModel model, Expression<Func<TModel, bool>> predicate) where TModel : class, new();
    }
}
