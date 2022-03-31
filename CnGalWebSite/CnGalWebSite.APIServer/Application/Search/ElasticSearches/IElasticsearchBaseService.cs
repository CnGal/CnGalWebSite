using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Search.ElasticSearches
{
    public interface IElasticsearchBaseService<TEntity> where TEntity : class
    {
        /// <summary>
        /// 获取索引
        /// </summary>
        /// <returns></returns>
        string GetIndex();

        /// <summary>
        ///     是否存在索引
        /// </summary>
        Task<bool> IndexExistsAsync();
        /// <summary>
        ///     新增数据
        /// </summary>
        /// <param name="entity"></param>
        Task InsertAsync(TEntity entity);

        /// <summary>
        ///     批量新增
        /// </summary>
        /// <param name="entity"></param>
        Task InsertRangeAsync(IEnumerable<TEntity> entity);

        /// <summary>
        /// 根据索引删除数据
        /// </summary>
        /// <returns></returns>
        Task RemoveIndex();
        /// <summary>
        /// 根据索引删除数据 ID
        /// </summary>
        /// <param name="Id">实体ID</param>
        /// <returns></returns>
        Task DeleteAsync(string Id);
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task UpdateAsync(TEntity entity, string Id);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="page">当前页码 从1编号 或 当前索引 从0编号</param>
        /// <param name="limit">每页数量 或 请求项目数</param>
        /// <param name="Text">搜索字符串</param>
        /// <param name="type">分页方式</param>
        Task<Tuple<int, IList<TEntity>>> QueryAsync(int page, int limit, string Text, string sort, SortOrder sortOrder, Func<BoolQueryDescriptor<TEntity>, IBoolQuery> field, QueryType type);
    }

}
