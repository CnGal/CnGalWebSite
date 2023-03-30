using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using System;
using CnGalWebSite.DrawingBed.Models.ViewModels;

namespace CnGalWebSite.DrawingBed.Services
{
    public interface IQueryService
    {
        Task<(IQueryable<TModel>, int)> QueryAsync<TModel, Tkey>(IQueryable<TModel> _query, QueryParameterModel model, Expression<Func<TModel, bool>> predicate) where TModel : class, new();
    }
}
