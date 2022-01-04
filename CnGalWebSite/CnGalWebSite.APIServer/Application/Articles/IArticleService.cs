
using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Articles.Dtos;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Search;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Articles
{
    public interface IArticleService
    {
        Task<PagedResultDto<Article>> GetPaginatedResult(GetArticleInput input);

        Task<QueryData<ListArticleAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListArticleAloneModel searchModel);

        Task<PagedResultDto<ArticleInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input);

        void UpdateArticleDataMain(Article article, ArticleMain examine);

        void UpdateArticleDataRelevances(Article article, ArticleRelecancesModel examine);

        void UpdateArticleDataMainPage(Article article, string examine);

        void UpdateArticleData(Article article, Examine examine);
    }
}
