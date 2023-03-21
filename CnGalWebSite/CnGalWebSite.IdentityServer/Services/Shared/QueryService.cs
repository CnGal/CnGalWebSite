using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Models.ViewModels.Shared;
using CnGalWebSite.IdentityServer.Models.ViewModels.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using IdentityModel;
using System.Linq.Expressions;
using System;

namespace CnGalWebSite.IdentityServer.Services.Shared
{
    public class QueryService:IQueryService
    {
        public async Task<(IQueryable<TModel>,int)> QueryAsync<TModel,Tkey>(IQueryable<TModel> _query, QueryParameterModel model, Expression<Func<TModel, bool>> predicate) where TModel : class,new()
        {
            var sortBy = model.SortBy.ToArray();
            var sortDesc = model.SortDesc.ToArray();
            var page = model.Page;
            var itemsPerPage = model.ItemsPerPage;

            var items = _query.AsSingleQuery();
            //搜索
           items = items.Where(predicate);

            //计算总数
            var total = await items.CountAsync();

            //排序
            var sb = new StringBuilder();
            for (int i = 0; i < sortBy.Length; i++)
            {
                sb.Append($"{(i == 0 ? "" : ", ")}{sortBy[i]}{(sortDesc[i] ? " desc" : "")}");
            }
            if (sb.Length != 0)
            {
                items = items.OrderBy(sb.ToString());
            }

            //分页
            if (itemsPerPage > 0)
            {
                items = items.Skip((page - 1) * itemsPerPage).Take(itemsPerPage);
            }

            return (items,total);
        }
    }
}
