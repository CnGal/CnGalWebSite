
using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Articles.Dtos;

using CnGalWebSite.DataModel.ExamineModel.Articles;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Search;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Articles
{
    public interface IArticleService
    {
        Task<PagedResultDto<Article>> GetPaginatedResult(GetArticleInput input);

        Task<PagedResultDto<ArticleInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input);

        void UpdateArticleDataMain(Article article, ExamineMain examine);
        void UpdateArticleDataMain(Article article, ArticleMain_1_0 examine);

        Task UpdateArticleDataRelevances(Article article, ArticleRelevances examine);

        void UpdateArticleDataMainPage(Article article, string examine);

        Task UpdateArticleData(Article article, Examine examine);

        Task<List<long>> GetArticleIdsFromNames(List<string> names);

        Task<NewsModel> GetNewsModelAsync(Article article);

        ArticleViewModel GetArticleViewModelAsync(Article article, bool renderMarkdown = true);

        List<KeyValuePair<object, Operation>> ExaminesCompletion(Article currentArticle, Article newArticle);

        List<ArticleViewModel> ConcompareAndGenerateModel(Article currentArticle, Article newArticle);

        Task<ArticleEditState> GetArticleEditState(ApplicationUser user, long id);

        void SetDataFromEditArticleMainViewModel(Article newArticle, EditArticleMainViewModel model);

        void SetDataFromEditArticleMainPageViewModel(Article newArticle, EditArticleMainPageViewModel model);

        void SetDataFromEditArticleRelevancesViewModel(Article newArticle, EditArticleRelevancesViewModel model, List<Entry> entries, List<Article> articles, List<Video> videos);

        Task<bool> CanUserEditArticleAsync(ApplicationUser user, long articleId);
    }
}
