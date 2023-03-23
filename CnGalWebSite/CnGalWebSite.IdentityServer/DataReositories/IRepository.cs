using IdentityModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.DataReositories
{
    /// <summary>
    /// 此接口是所有仓储的约定，此接口仅作为约定，用于标识它们
    /// </summary>
    /// <typeparam name="TEntity">当前传入的仓储的实体类型</typeparam>
    /// <typeparam name="TPrimaryKey">传入仓储的主键类型</typeparam>
    /// <typeparam name="TDbContext">数据库</typeparam>
    public interface IRepository<TDbContext,TEntity, TPrimaryKey> where TEntity : class where TDbContext : DbContext
    {
        #region 查询
        /// <summary>
        /// 获取用于从整个表中检索实体的IQueryable
        /// </summary>
        /// <returns>可用于从数据库中选择实体</returns>
        IQueryable<TEntity> GetAll();

        /// <summary>
        /// 用于获取所有实体
        /// </summary>
        /// <returns>所有实体列表</returns>
        List<TEntity> GetAllList();

        /// <summary>
        /// 用于获取所有实体的异步实现
        /// </summary>
        /// <returns>所有实体列表</returns>
        Task<List<TEntity>> GetAllListAsync();

        /// <summary>
        /// 用于获取传入本方法的所有实体
        /// </summary>
        /// <param name="predicate">筛选实体的条件</param>
        /// <returns>所有实体列表</returns>
        List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 用于获取传入本方法的所有实体
        /// </summary>
        /// <param name="predicate">筛选实体的条件</param>
        /// <returns>所有实体列表</returns>
        Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 通过传入的筛选条件来获取实体信息
        /// 如果查询不到返回值 则会引发异常
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <returns>符合的实体</returns>
        TEntity Single(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 通过传入的筛选条件来获取实体信息
        /// 如果查询不到返回值 则会引发异常
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <returns>符合的实体</returns>
        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 通过传入的筛选条件查询实体信息 如果没有找到 则返回null
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 通过传入的筛选条件查询实体信息 如果没有找到 则返回null
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        #endregion

        #region 添加
        /// <summary>
        /// 添加一个新的实体信息
        /// </summary>
        /// <param name="entity">被添加的实体</param>
        /// <returns>被添加的实体</returns>
        TEntity Insert(TEntity entity);

        /// <summary>
        /// 添加一个新的实体信息
        /// </summary>
        /// <param name="entity">被添加的实体</param>
        /// <returns>被添加的实体</returns>
        Task<TEntity> InsertAsync(TEntity entity);
        #endregion

        #region 更新
        /// <summary>
        /// 更新现有实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>被更新的实体</returns>
        TEntity Update(TEntity entity);

        /// <summary>
        /// 更新现有实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>被更新的实体</returns>
        Task<TEntity> UpdateAsync(TEntity entity);
 
        #endregion

        #region 删除

        /// <summary>
        /// 删除一个实体
        /// </summary>
        /// <param name="entity">实体</param>
        void Delete(TEntity entity);

        /// <summary>
        /// 删除一个实体
        /// </summary>
        /// <param name="entity">实体</param>
        Task DeleteAsync(TEntity entity);

        /// <summary>
        /// 按传入的条件可删除多个实体
        /// 注意：所有符合给定条件的实体都将被检索和删除
        /// 如果条件比较多，则待删除的实体页比较多，这可能会导致主要的性能问题
        /// </summary>
        /// <param name="predicarte">筛选实体的条件</param>
        void Delete(Expression<Func<TEntity, bool>> predicarte);

        /// <summary>
        /// 按传入的条件可删除多个实体
        /// 注意：所有符合给定条件的实体都将被检索和删除
        /// 如果条件比较多，则待删除的实体页比较多，这可能会导致主要的性能问题
        /// </summary>
        /// <param name="predicarte">筛选实体的条件</param>
        Task DeleteAsync(Expression<Func<TEntity, bool>> predicarte);

        #endregion

        #region 总和计算
        /// <summary>
        /// 获取此仓储中所有实体的总和
        /// </summary>
        /// <returns>实体的总数</returns>
        int Count();

        /// <summary>
        /// 获取此仓储中所有实体的总和
        /// </summary>
        /// <returns>实体的总数</returns>
        Task<int> CountAsync();

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 支持条件筛选 计算仓储中的实体总和
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <returns>实体的总数</returns>
        int Count(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 支持条件筛选 计算仓储中的实体总和
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <returns>实体的总数</returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 获取此仓储中所有实体得到总和 如果预期返回值大于Int.MaxValue 值，则推荐
        /// 该方法
        /// </summary>
        /// <returns>实体总数</returns>
        long LongCount();

        /// <summary>
        /// 获取此仓储中所有实体得到总和 如果预期返回值大于Int.MaxValue 值，则推荐
        /// 该方法
        /// </summary>
        /// <returns>实体总数</returns>
        Task<long> LongCountAsync();

        /// <summary>
        /// 获取此仓储中所有实体得到总和 如果预期返回值大于Int.MaxValue 值，则推荐
        /// 该方法
        /// </summary>
        /// /// <param name="predicate">条件</param>
        /// <returns>实体总数</returns>
        long LongCount(Expression<Func<TEntity, bool>> predicate);
        /// <summary>
        /// 获取此仓储中所有实体得到总和 如果预期返回值大于Int.MaxValue 值，则推荐
        /// 该方法
        /// </summary>
        /// /// <param name="predicate">条件</param>
        /// <returns>实体总数</returns>
        Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate);
        #endregion

        #region 清除

        /// <summary>
        /// 清除上下文中的跟踪状态
        /// </summary>
        void Clear();
        #endregion
    }
}
