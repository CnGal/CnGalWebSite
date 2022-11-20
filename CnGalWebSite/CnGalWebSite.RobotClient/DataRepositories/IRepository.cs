using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient.DataRepositories
{
    /// <summary>
    /// 此接口是所有仓储的约定，此接口仅作为约定，用于标识它们
    /// </summary>
    /// <typeparam name="TEntity">当前传入的仓储的实体类型</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        //获取数据
        List<TEntity> GetAll();

        //插入
        TEntity Insert(TEntity entity);

        //保存到文件
        void Save();

        //删除
        void Delete(TEntity entity);
    }
}
