using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.News
{
    public interface INewsService
    {
        Task UpdateNewestGameNews();

        Task UpdateWeiboUserInforCache();

        Task<WeeklyNews> GenerateNewestWeeklyNews();

        Task PublishNews(GameNews gameNews);

        Task PublishWeeklyNews(WeeklyNews weeklyNews);

        Task<WeeklyNews> ResetWeeklyNews(WeeklyNews weeklyNews);

        Task AddWeiboUserInfor(string entryName, long weiboId);

        string GenerateRealWeeklyNewsMainPage(WeeklyNews weeklyNews);

        Task<Article> GameNewsToArticle(GameNews gameNews);

        Task<Article> WeeklyNewsToArticle(WeeklyNews weeklyNews);

        Task AddGameMewsFromWeibo(long id, string keyword);

        Task<QueryData<ListGameNewAloneModel>> GetPaginatedResult(QueryPageOptions options, ListGameNewAloneModel searchModel);

        Task<QueryData<ListWeeklyNewAloneModel>> GetPaginatedResult(QueryPageOptions options, ListWeeklyNewAloneModel searchModel);
    }
}
