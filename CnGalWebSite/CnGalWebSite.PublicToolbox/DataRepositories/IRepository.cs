using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PublicToolbox.DataRepositories
{
    /// <summary>
    /// 此接口是所有仓储的约定，此接口仅作为约定，用于标识它们
    /// </summary>
    /// <typeparam name="TEntity">当前传入的仓储的实体类型</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        List<TEntity> GetAll();

        Task<TEntity> InsertAsync(TEntity entity);

        Task SaveAsync();

        Task DeleteAsync(TEntity entity);
    }
}
