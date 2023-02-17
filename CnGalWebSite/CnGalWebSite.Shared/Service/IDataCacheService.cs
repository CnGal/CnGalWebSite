using BlazorComponent;
using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Anniversaries;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.DataModel.ViewModel.DelayedTasks;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.ThematicPages;
using CnGalWebSite.DataModel.ViewModel.Theme;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.DataModel.ViewModel.Votes;
using CnGalWebSite.Helper.ViewModel.Articles;
using CnGalWebSite.Helper.ViewModel.Comments;
using CnGalWebSite.Helper.ViewModel.Users;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IDataCacheService
    {
        bool IsApp { get; set; }

        bool IsMiniMode { get; set; }

        event EventHandler<EventArgs> RefreshRequsted;

        string LoginKey { get; set; }

        ThirdPartyLoginTempModel ThirdPartyLoginTempModel { get; set; }

        string UserName { get; set; }

        string UserEmail { get; set; }

        bool IsOnThirdPartyLogin { get; set; }

        UserAuthenticationTypeModel UserAuthenticationTypeModel { get; set; }

        HomeViewModel HomeViewModel { get; set; }

        int RenderTimes { get; set; }

        CommentViewModel DetailComment { get; set; }

        UserInforViewModel UserInfor { get; set; }

        ThemeModel ThemeSetting { get; set; }

        List<DelayedTask> DelayedTaskList { get; set; }

        IPageModelCatche<EntryIndexViewModel> EntryIndexPageCatche { get; set; }

        IPageModelCatche<PeripheryViewModel> PeripheryIndexPageCatche { get; set; }

        IPageModelCatche<TagIndexViewModel> TagIndexPageCatche { get; set; }

        IPageModelCatche<ArticleViewModel> ArticleIndexPageCatche { get; set; }

        IPageModelCatche<VideoViewModel> VideoIndexPageCatche { get; set; }

        IPageModelCatche<List<HomeNewsAloneViewModel>> HomePageNewsCatche { get; set; }

        IPageModelCatche<List<CarouselViewModel>> HomePageCarouselsCatche { get; set; }

        IPageModelCatche<VoteViewModel> VoteIndexPageCatche { get; set; }

        IPageModelCatche<FavoriteFolderViewModel> FavoriteFolderIndexPageCatche { get; set; }

        IPageModelCatche<ExaminesOverviewViewModel> ExaminesOverviewCatche { get; set; }

        IPageModelCatche<EntryContrastEditRecordViewModel> EntryContrastEditRecordViewCatche { get; set; }

        IPageModelCatche<ArticleContrastEditRecordViewModel> ArticleContrastEditRecordViewCatche { get; set; }

        IPageModelCatche<PeripheryContrastEditRecordViewModel> PeripheryContrastEditRecordViewCatche { get; set; }

        IPageModelCatche<VideoContrastEditRecordViewModel> VideoContrastEditRecordViewCatche { get; set; }

        IPageModelCatche<TagContrastEditRecordViewModel> TagContrastEditRecordViewCatche { get; set; }

        IPageModelCatche<LotteryViewModel> LotteryIndexPageCatche { get; set; }

        IPageModelCatche<LineChartModel> LineChartDataCatche { get; set; }

        IPageModelCatche<PlayedGameOverviewModel> PlayedGameOverviewDataCatche { get; set; }

        IPageModelCatche<CommentCacheModel> CommentDataCatche { get; set; }

        IPageModelCatche<PersonalSpaceViewModel> PersonalSpaceDataCatche { get; set; }

        IPageModelCatche<List<GameRecordViewModel>> UserGameRecordDataCatche { get; set; }

        IPageModelCatche<List<SteamUserInfor>> UserSteamInforDataCatche { get; set; }

        IPageModelCatche<UserVideoListModel> UserVideoListDataCatche { get; set; }

        IPageModelCatche<UserArticleListModel> UserArticleListDataCatche { get; set; }

        IPageModelCatche<PagedResultDto<ExaminedNormalListModel>> UserExaminesDataCatche { get; set; }

        IPageModelCatche<FavoriteFoldersViewModel> UserFavoriteFoldersDataCatche { get; set; }

        IPageModelCatche<PagedResultDto<FavoriteObjectAloneViewModel>> UserFavoriteObjectsDataCatche { get; set; }

        IPageModelCatche<List<EntryInforTipViewModel>> PublishGameTimesDataCatche { get; set; }

        IPageModelCatche<List<RoleBrithdayViewModel>> RoleBrithdaysDataCatche { get; set; }

        IPageModelCatche<SearchViewModel> SearchViewCatche { get; set; }

        IPageModelCatche<List<MainImageCardModel>> HomeListCardsCache { get; set; }

        IPageModelCatche<List<PublishGamesTimelineModel>> PublishGamesTimelineDataCatche { get; set; }

        UserPendingDataCacheModel UserPendingDataCatche { get; set; }

        List<DocumentViewModel> DocumentsCatche { get; set; }

        List<RandomTagModel> RandomTagsCatche { get; set; }

        List<SteamInforTipViewModel> DiscountGamesCatche { get; set; }

        List<ArticleInforTipViewModel> RandomArticlesCatche { get; set; }

        List<VoteCardViewModel> VoteCardsCatche { get; set; }

        List<LotteryCardViewModel> LotteryCardsCatche { get; set; }

        List<GameCGModel> GameCGsCatche { get; set; }

        List<EntryInforTipViewModel> FreeGamesCatche { get; set; }

        List<ArticleInforTipViewModel> WeeklyNewsOverviewCatche { get; set; }

        List<GameEvaluationsModel> GameEvaluationsCatche { get; set; }

        List<GameRoleModel> GameRolesCatche { get; set; }

        NewsSummaryCacheModel NewsSummaryCache { get; set; }

        List<JudgableGameViewModel> JudgableGamesCatche { get; set; }

        List<PlayedGameUserScoreRandomModel> RandomUserScoresCatche { get; set; }

        public UserContentCenterViewModel UserContentCenterCatche { get; set; }


        DiscountPageHelper DiscountPageCatcheHelper { get; set; }

        SpaceViewCacheModel SpaceViewCache { get; set; }

        CVThematicPageViewModel CVThematicPageCache { get; set; }

        StringNumber HomeTabIndex { get; set; }

        void RefreshAllCatche();

        Task OnRefreshRequsted(EventArgs e);
    }
}
