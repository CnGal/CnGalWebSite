using System;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.Storage
{
    /// <summary>
    /// 持久化存储接口
    /// </summary>
    public interface IPersistentStorage
    {
        /// <summary>
        /// 保存数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="data">数据</param>
        /// <param name="expiration">过期时间（可选）</param>
        Task SaveAsync<T>(string key, T data, TimeSpan? expiration = null);

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>数据，如果不存在则返回null</returns>
        Task<T?> LoadAsync<T>(string key);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="key">键</param>
        Task DeleteAsync(string key);

        /// <summary>
        /// 检查数据是否存在
        /// </summary>
        /// <param name="key">键</param>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 清理过期数据
        /// </summary>
        Task CleanupExpiredAsync();
    }
}
