﻿
using CnGalWebSite.Components.Service;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Anniversaries;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.DataModel.ViewModel.DelayedTasks;
using CnGalWebSite.DataModel.ViewModel.Dtos;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Stores;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.ThematicPages;
using CnGalWebSite.DataModel.ViewModel.Theme;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.DataModel.ViewModel.Votes;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.Helper.ViewModel.Comments;
using CnGalWebSite.Shared.Models.Articles;
using CnGalWebSite.Shared.Models.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public class DataCatcheService : IDataCacheService
    {
        /// <summary>
        /// 是否为APP模式
        /// </summary>
        public bool IsApp { get; set; }
        /// <summary>
        /// 是否为精简模式
        /// </summary>
        public bool IsMiniMode { get; set; }
   
        /// <summary>
        /// 主页渲染次数
        /// </summary>
        public int RenderTimes { get; set; } = 0;
        /// <summary>
        /// 主题设置
        /// </summary>
        public ThemeModel ThemeSetting { get; set; } = new ThemeModel();
        /// <summary>
        /// 延迟任务列表
        /// </summary>
        public List<DelayedTask> DelayedTaskList { get; set; } = new List<DelayedTask>();

        /// <summary>
        /// 词条主页数据缓存
        /// </summary>
        public IPageModelCatche<EntryIndexViewModel> EntryIndexPageCatche { get; set; }
        /// <summary>
        /// 文章主页数据缓存
        /// </summary>
        public IPageModelCatche<ArticleViewModel> ArticleIndexPageCatche { get; set; }
        /// <summary>
        /// 周边主页数据缓存
        /// </summary>
        public IPageModelCatche<PeripheryViewModel> PeripheryIndexPageCatche { get; set; }
        /// <summary>
        /// 投票主页数据缓存
        /// </summary>
        public IPageModelCatche<VoteViewModel> VoteIndexPageCatche { get; set; }
        /// <summary>
        /// 抽奖主页数据缓存
        /// </summary>
        public IPageModelCatche<LotteryViewModel> LotteryIndexPageCatche { get; set; }
        /// <summary>
        /// 视频主页数据缓存
        /// </summary>
        public IPageModelCatche<VideoViewModel> VideoIndexPageCatche { get; set; }
        /// <summary>
        /// 目录主页数据缓存
        /// </summary>
        public IPageModelCatche<FavoriteFolderViewModel> FavoriteFolderIndexPageCatche { get; set; }

        /// <summary>
        /// 标签主页数据缓存
        /// </summary>
        public IPageModelCatche<TagIndexViewModel> TagIndexPageCatche { get; set; }
        /// <summary>
        /// 编辑总览页面数据缓存
        /// </summary>
        public IPageModelCatche<ExaminesOverviewViewModel> ExaminesOverviewCatche { get; set; }
        /// <summary>
        /// 编辑对比页面数据缓存 词条
        /// </summary>
        public IPageModelCatche<EntryContrastEditRecordViewModel> EntryContrastEditRecordViewCatche { get; set; }
        /// <summary>
        /// 编辑对比页面数据缓存 文章
        /// </summary>
        public IPageModelCatche<ArticleContrastEditRecordViewModel> ArticleContrastEditRecordViewCatche { get; set; }
        /// <summary>
        /// 编辑对比页面数据缓存 周边
        /// </summary>
        public IPageModelCatche<PeripheryContrastEditRecordViewModel> PeripheryContrastEditRecordViewCatche { get; set; }
        /// <summary>
        /// 编辑对比页面数据缓存 标签
        /// </summary>
        public IPageModelCatche<TagContrastEditRecordViewModel> TagContrastEditRecordViewCatche { get; set; }
        /// <summary>
        /// 编辑对比页面数据缓存 视频
        /// </summary>
        public IPageModelCatche<VideoContrastEditRecordViewModel> VideoContrastEditRecordViewCatche { get; set; }
        /// <summary>
        /// 图表缓存
        /// </summary>
        public IPageModelCatche<LineChartModel> LineChartDataCatche { get; set; }
        /// <summary>
        /// 用户热力图缓存
        /// </summary>
        public IPageModelCatche<EChartsHeatMapOptionModel> HeatMapDataCatche { get; set; }
        /// <summary>
        /// 游玩记录缓存
        /// </summary>
        public IPageModelCatche<PlayedGameOverviewModel> PlayedGameOverviewDataCatche { get; set; }
        /// <summary>
        /// 评论缓存
        /// </summary>
        public IPageModelCatche<CommentCacheModel> CommentDataCatche { get; set; }
        /// <summary>
        /// 用户个人空间缓存
        /// </summary>
        public IPageModelCatche<PersonalSpaceViewModel> PersonalSpaceDataCatche { get; set; }
        /// <summary>
        /// 用户游玩记录列表缓存
        /// </summary>
        public IPageModelCatche<List<GameRecordViewModel>> UserGameRecordDataCatche { get; set; }
        /// <summary>
        /// 用户Steam信息缓存
        /// </summary>
        public IPageModelCatche<List<SteamUserInforModel>> UserSteamInforDataCatche { get; set; }
        /// <summary>
        /// 用户文章列表缓存
        /// </summary>
        public IPageModelCatche<UserArticleListModel> UserArticleListDataCatche { get; set; }
        /// <summary>
        /// 用户视频列表缓存
        /// </summary>
        public IPageModelCatche<UserVideoListModel> UserVideoListDataCatche { get; set; }
        /// <summary>
        /// 用户编辑记录列表缓存
        /// </summary>
        public IPageModelCatche<PagedResultDto<ExaminedNormalListModel>> UserExaminesDataCatche { get; set; }
        /// <summary>
        /// 用户收藏对象列表缓存
        /// </summary>
        public IPageModelCatche<PagedResultDto<FavoriteObjectAloneViewModel>> UserFavoriteObjectsDataCatche { get; set; }
        /// <summary>
        /// 游戏发售时间轴缓存
        /// </summary>
        public IPageModelCatche<List<PublishGamesTimelineModel>> PublishGamesTimelineDataCatche { get; set; }
        /// <summary>
        /// 角色生日列表缓存
        /// </summary>
        public IPageModelCatche<List<RoleBrithdayViewModel>> RoleBrithdaysDataCatche { get; set; }
        /// <summary>
        /// 主页缓存
        /// </summary>
        public IPageModelCatche<List<HomeItemModel>> HomeListCardsCache { get; set; }

        /// <summary>
        /// 用户待审核对象列表缓存
        /// </summary>
        public UserPendingDataCacheModel UserPendingDataCatche { get; set; } = new UserPendingDataCacheModel();
        /// <summary>
        /// 文档缓存
        /// </summary>
        public List<DocumentViewModel> DocumentsCatche { get; set; } = new List<DocumentViewModel>();
        /// <summary>
        /// 随机标签
        /// </summary>
        public List<RandomTagModel> RandomTagsCatche { get; set; } = new List<RandomTagModel>();

        /// <summary>
        /// 广场抽奖列表缓存
        /// </summary>
        public List<LotteryCardViewModel> LotteryCardsCatche { get; set; } = new List<LotteryCardViewModel>();
        /// <summary>
        /// 广场投票列表缓存
        /// </summary>
        public List<VoteCardViewModel> VoteCardsCatche { get; set; } = new List<VoteCardViewModel>();

        /// <summary>
        /// 折扣页面缓存辅助类
        /// </summary>
        public DiscountPageHelper DiscountPageCatcheHelper { get; set; } = new DiscountPageHelper();
        /// <summary>
        /// 可评选游戏缓存
        /// </summary>
        public List<JudgableGameViewModel> JudgableGamesCatche { get; set; } = new List<JudgableGameViewModel>();
        /// <summary>
        /// 随机用户评分
        /// </summary>
        public List<PlayedGameUserScoreRandomModel> RandomUserScoresCatche { get; set; } = new List<PlayedGameUserScoreRandomModel>();
        /// <summary>
        /// 内容中心缓存
        /// </summary>
        public UserContentCenterViewModel UserContentCenterCatche { get; set; }

        /// <summary>
        /// 评论详情
        /// </summary>
        public CommentViewModel DetailComment { get; set; } = new CommentViewModel();
        /// <summary>
        /// 个人主页缓存
        /// </summary>
        public SpaceViewCacheModel SpaceViewCache { get; set; } = new SpaceViewCacheModel();
        /// <summary>
        /// 动态汇总缓存
        /// </summary>
        public NewsSummaryCacheModel NewsSummaryCache { get; set; } = new NewsSummaryCacheModel();
        /// <summary>
        /// 当前登入的用户的信息
        /// </summary>
        public UserInforViewModel UserInfor { get; set; } = new UserInforViewModel { Ranks = new List<RankViewModel>() };
        /// <summary>
        /// CV专题页缓存
        /// </summary>
        public CVThematicPageViewModel CVThematicPageCache { get; set; }

        /// <summary>
        /// 游戏销量
        /// </summary>
        public GameRevenueInfoCacheModel GameRevenueInfoCache { get; set; } = new();


        /// <summary>
        /// 主页Tab
        /// </summary>
        public int HomeTabIndex { get; set; }

        /// <summary>
        /// 搜索页面缓存
        /// </summary>
        public IPageModelCatche<SearchViewModel> SearchViewCatche { get; set; }

        public DataCatcheService(IPageModelCatche<EntryIndexViewModel> entryIndexPageCatche, IPageModelCatche<ArticleViewModel> articleIndexPageCatche, IPageModelCatche<VoteViewModel> voteIndexPageCatche,
        IPageModelCatche<PeripheryViewModel> peripheryIndexPageCatche, IPageModelCatche<TagIndexViewModel> tagIndexPageCatche, IPageModelCatche<List<HomeNewsAloneViewModel>> homePageNewsCatche,
        IPageModelCatche<List<CarouselViewModel>> homePageCarouselsCatche, IPageModelCatche<ExaminesOverviewViewModel> examinesOverviewCatche,
        IPageModelCatche<LotteryViewModel> lotteryIndexPageCatche,
        IPageModelCatche<ArticleContrastEditRecordViewModel> articleContrastEditRecordViewCatche,
        IPageModelCatche<PeripheryContrastEditRecordViewModel> peripheryContrastEditRecordViewCatche,
        IPageModelCatche<TagContrastEditRecordViewModel> tagContrastEditRecordViewCatche,
         IPageModelCatche<VideoContrastEditRecordViewModel> videoContrastEditRecordViewCatche,
        IPageModelCatche<EntryContrastEditRecordViewModel> entryContrastEditRecordViewCatche,
        IPageModelCatche<SearchViewModel> searchViewCatche,
        IPageModelCatche<LineChartModel> lineChartDataCatche,
        IPageModelCatche<CommentCacheModel> commentDataCatche,
        IPageModelCatche<PersonalSpaceViewModel> personalSpaceDataCatche,
        IPageModelCatche<List<GameRecordViewModel>> userGameRecordDataCatche,
        IPageModelCatche<List<SteamUserInforModel>> userSteamInforDataCatche,
        IPageModelCatche<UserArticleListModel> userArticleListDataCatche,
        IPageModelCatche<UserVideoListModel> userVideoListDataCatche,
        IPageModelCatche<PagedResultDto<ExaminedNormalListModel>> userExaminesDataCatche,
        IPageModelCatche<PagedResultDto<FavoriteObjectAloneViewModel>> userFavoriteObjectsDataCatche,
        IPageModelCatche<PlayedGameOverviewModel> playedGameOverviewDataCatche,
        IPageModelCatche<List<HomeItemModel>> homeListCardsCache,
        IPageModelCatche<VideoViewModel> videoIndexPageCatche,
        IPageModelCatche<List<RoleBrithdayViewModel>> roleBrithdaysDataCatche,
        IPageModelCatche<FavoriteFolderViewModel> favoriteFolderIndexPageCatche,
         IPageModelCatche<List<PublishGamesTimelineModel>> publishGamesTimelineDataCatche, IPageModelCatche<EChartsHeatMapOptionModel> heatMapDataCatche)
        {
            (EntryIndexPageCatche = entryIndexPageCatche).Init(nameof(EntryIndexPageCatche), ToolHelper.WebApiPath + "api/entries/GetEntryView/");
            (ArticleIndexPageCatche = articleIndexPageCatche).Init(nameof(ArticleIndexPageCatche), ToolHelper.WebApiPath + "api/articles/GetArticleView/");
            (PeripheryIndexPageCatche = peripheryIndexPageCatche).Init(nameof(PeripheryIndexPageCatche), ToolHelper.WebApiPath + "api/peripheries/GetPeripheryView/");
            (VoteIndexPageCatche = voteIndexPageCatche).Init(nameof(VoteIndexPageCatche), ToolHelper.WebApiPath + "api/votes/GetVoteView/");
            (LotteryIndexPageCatche = lotteryIndexPageCatche).Init(nameof(LotteryIndexPageCatche), ToolHelper.WebApiPath + "api/lotteries/GetLotteryView/");
            (VideoIndexPageCatche = videoIndexPageCatche).Init(nameof(VideoIndexPageCatche), ToolHelper.WebApiPath + "api/videos/GetView/");
            (FavoriteFolderIndexPageCatche = favoriteFolderIndexPageCatche).Init(nameof(FavoriteFolderIndexPageCatche), ToolHelper.WebApiPath + "api/Favorites/GetView/");
            (TagIndexPageCatche = tagIndexPageCatche).Init(nameof(TagIndexPageCatche), ToolHelper.WebApiPath + "api/tags/gettag/");
            (ExaminesOverviewCatche = examinesOverviewCatche).Init(nameof(ExaminesOverviewCatche), ToolHelper.WebApiPath + "api/examines/GetExaminesOverview/");
            (EntryContrastEditRecordViewCatche = entryContrastEditRecordViewCatche).Init(nameof(EntryContrastEditRecordViewCatche), ToolHelper.WebApiPath + "api/entries/GetContrastEditRecordViews/");
            (ArticleContrastEditRecordViewCatche = articleContrastEditRecordViewCatche).Init(nameof(ArticleContrastEditRecordViewCatche), ToolHelper.WebApiPath + "api/articles/GetContrastEditRecordViews/");
            (PeripheryContrastEditRecordViewCatche = peripheryContrastEditRecordViewCatche).Init(nameof(PeripheryContrastEditRecordViewCatche), ToolHelper.WebApiPath + "api/peripheries/GetContrastEditRecordViews/");
            (TagContrastEditRecordViewCatche = tagContrastEditRecordViewCatche).Init(nameof(TagContrastEditRecordViewCatche), ToolHelper.WebApiPath + "api/tags/GetContrastEditRecordViews/");
            (VideoContrastEditRecordViewCatche = videoContrastEditRecordViewCatche).Init(nameof(VideoContrastEditRecordViewCatche), ToolHelper.WebApiPath + "api/videos/GetContrastEditRecordViews/");
            (PlayedGameOverviewDataCatche = playedGameOverviewDataCatche).Init(nameof(PlayedGameOverviewDataCatche), ToolHelper.WebApiPath + "api/playedgame/GetPlayedGameOverview/");
            (CommentDataCatche = commentDataCatche).Init(nameof(CommentDataCatche), ToolHelper.WebApiPath + "api/comments/GetComments/");
            (PersonalSpaceDataCatche = personalSpaceDataCatche).Init(nameof(PersonalSpaceDataCatche), ToolHelper.WebApiPath + "api/space/getuserview/");
            (UserGameRecordDataCatche = userGameRecordDataCatche).Init(nameof(UserGameRecordDataCatche), ToolHelper.WebApiPath + "api/playedgame/GetUserGameRecords/");
            (UserSteamInforDataCatche = userSteamInforDataCatche).Init(nameof(UserSteamInforDataCatche), ToolHelper.WebApiPath + "api/steam/GetUserSteamInfor/");
            (UserArticleListDataCatche = userArticleListDataCatche).Init(nameof(UserArticleListDataCatche), ToolHelper.WebApiPath + "api/space/GetUserArticles/");
            (UserVideoListDataCatche = userVideoListDataCatche).Init(nameof(UserVideoListDataCatche), ToolHelper.WebApiPath + "api/space/GetUserVideos/");
            (UserExaminesDataCatche = userExaminesDataCatche).Init(nameof(UserExaminesDataCatche), ToolHelper.WebApiPath + "api/space/GetUserEditRecord");
            (UserFavoriteObjectsDataCatche = userFavoriteObjectsDataCatche).Init(nameof(UserFavoriteObjectsDataCatche), ToolHelper.WebApiPath + "api/favorites/GetUserFavoriteObjectList");
            (PublishGamesTimelineDataCatche = publishGamesTimelineDataCatche).Init(nameof(PublishGamesTimelineDataCatche), ToolHelper.WebApiPath + "api/entries/GetPublishGamesTimeline");
            (RoleBrithdaysDataCatche = roleBrithdaysDataCatche).Init(nameof(RoleBrithdaysDataCatche), ToolHelper.WebApiPath + "api/entries/GetRoleBirthdaysByTime");
            (HomeListCardsCache = homeListCardsCache).Init(nameof(HomeListCardsCache), ToolHelper.WebApiPath);

            (LineChartDataCatche = lineChartDataCatche).Init(nameof(LineChartDataCatche), "");
            (SearchViewCatche = searchViewCatche).Init(nameof(SearchViewCatche), "");
            (HeatMapDataCatche = heatMapDataCatche).Init(nameof(HeatMapDataCatche), ""); ;
        }

        public void RefreshAllCatche()
        {
            EntryIndexPageCatche.Clean();
            ArticleIndexPageCatche.Clean();
            PeripheryIndexPageCatche.Clean();
            VoteIndexPageCatche.Clean();
            TagIndexPageCatche.Clean();
            PlayedGameOverviewDataCatche.Clean();

            UserContentCenterCatche = null;
            UserInfor = new UserInforViewModel { Ranks = new List<RankViewModel>() };
        }
    }
}
