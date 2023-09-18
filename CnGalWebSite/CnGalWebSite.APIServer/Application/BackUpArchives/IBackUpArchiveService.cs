
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.BackUpArchives
{
    public interface IBackUpArchiveService
    {
        /// <summary>
        /// 备份文章
        /// </summary>
        /// <param name="backUpArchive"></param>
        /// <returns></returns>
        Task BackUpArticle(BackUpArchive backUpArchive);
        /// <summary>
        /// 备份词条
        /// </summary>
        /// <param name="backUpArchive"></param>
        /// <param name="entryName"></param>
        /// <returns></returns>
        Task BackUpEntry(BackUpArchive backUpArchive, string entryName);
        /// <summary>
        /// 备份所有文章
        /// </summary>
        /// <param name="maxNum"></param>
        /// <returns></returns>
        Task BackUpAllArticles(int maxNum);
        /// <summary>
        /// 备份所有词条
        /// </summary>
        /// <param name="maxNum"></param>
        /// <returns></returns>
        Task BackUpAllEntries(int maxNum);

        /// <summary>
        /// 更新网站地图
        /// </summary>
        /// <returns></returns>
        Task UpdateSitemap();
    }
}
