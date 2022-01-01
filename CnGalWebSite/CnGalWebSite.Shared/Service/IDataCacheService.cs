using Microsoft.AspNetCore.Components;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Theme;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IDataCacheService
    {

        bool? IsApp { get; set; }

        EventCallback RefreshApp { get; set; }

        EventCallback SavaTheme { get; set; }

        string LoginKey { get; set; }

        ThirdPartyLoginTempModel ThirdPartyLoginTempModel { get; set; }

        string UserName { get; set; }

        bool IsOnThirdPartyLogin { get; set; }

        UserAuthenticationTypeModel UserAuthenticationTypeModel { get; set; }

        HomeViewModel HomeViewModel { get; set; }

        int RenderTimes { get; set; }

        CommentViewModel DetailComment { get; set; }

        UserInforViewModel UserInfor { get; set; }

        ThemeModel ThemeSetting { get; set; }

        IPageModelCatche<EntryIndexViewModel> EntryIndexPageCatche { get; set; }

        IPageModelCatche<PeripheryViewModel> PeripheryIndexPageCatche { get; set; }

        IPageModelCatche<TagIndexViewModel> TagIndexPageCatche { get; set; }

        IPageModelCatche<ArticleViewModel> ArticleIndexPageCatche { get; set; }

        IPageModelCatche<List<HomeNewsAloneViewModel>> HomePageNewsCatche { get; set; }

        IPageModelCatche<List<DataModel.Model.Carousel>> HomePageCarouselsCatche { get; set; }

        SearchViewModel SearchViewModel { get; set; }

        Task<List<CnGalWebSite.Shared.AppComponent.Normal.Cards.MainImageCardModel>> GetHomePageListCardMode(string apiUrl, string type, int maxCount, bool isRefresh);

        void RefreshSearchCatche();

    }
}
