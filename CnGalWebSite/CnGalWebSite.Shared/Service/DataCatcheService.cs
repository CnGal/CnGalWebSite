using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Application.Examines.Dtos;
using CnGalWebSite.DataModel.Helper;
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
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.ThematicPages;
using CnGalWebSite.DataModel.ViewModel.Theme;
using CnGalWebSite.DataModel.ViewModel.Votes;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.Helper.ViewModel.Articles;
using CnGalWebSite.Helper.ViewModel.Comments;
using CnGalWebSite.Helper.ViewModel.Users;
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

        public event EventHandler<EventArgs> RefreshRequsted;

        /// <summary>
        /// 刷新渲染框架方法
        /// </summary>
        public EventCallback RefreshApp { get; set; }
        /// <summary>
        /// 保存主题设置
        /// </summary>
        public EventCallback SavaTheme { get; set; }
        /// <summary>
        /// 新建标签页
        /// </summary>
        public EventCallback<string> OpenNewPage { get; set; }
        /// <summary>
        /// 主题更改 仅MAUI下生效
        /// </summary>
        public EventCallback<string> ThemeChanged { get; set; }
        /// <summary>
        /// 退出应用
        /// </summary>
        public EventCallback Quit { get; set; }

        /// <summary>
        /// 分享页面
        /// </summary>
        public EventCallback<ShareLinkModel> ShareLink { get; set; }
        /// <summary>
        /// 身份验证成功后获得的标识 有效期一小时
        /// </summary>
        public string LoginKey { get; set; } = string.Empty;
        /// <summary>
        /// 第三方登入成功 服务端也验证成功后 返回唯一标识 有效期一小时
        /// </summary>
        public ThirdPartyLoginTempModel ThirdPartyLoginTempModel { get; set; } = null;
        /// <summary>
        /// 历史用户临时储存用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        /// <summary>
        /// 是否正在使用第三方登入
        /// </summary>
        public bool IsOnThirdPartyLogin { get; set; } = true;
        /// <summary>
        /// 用户二次身份验证方式
        /// </summary>
        public UserAuthenticationTypeModel UserAuthenticationTypeModel { get; set; } = new UserAuthenticationTypeModel();
        /// <summary>
        /// 主页数据缓存
        /// </summary>
        public HomeViewModel HomeViewModel { get; set; } = null;
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
        /// 标签主页数据缓存
        /// </summary>
        public IPageModelCatche<TagIndexViewModel> TagIndexPageCatche { get; set; }
        /// <summary>
        /// 主页动态数据缓存
        /// </summary>
        public IPageModelCatche<List<HomeNewsAloneViewModel>> HomePageNewsCatche { get; set; }
        /// <summary>
        /// 主页轮播图数据缓存
        /// </summary>
        public IPageModelCatche<List<CarouselViewModel>> HomePageCarouselsCatche { get; set; }
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
        /// 图表缓存
        /// </summary>
        public IPageModelCatche<LineChartModel> LineChartDataCatche { get; set; }
        /// <summary>
        /// 图表缓存
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
        public IPageModelCatche<List<SteamUserInfor>> UserSteamInforDataCatche { get; set; }
        /// <summary>
        /// 用户文章列表缓存
        /// </summary>
        public IPageModelCatche<UserArticleListModel> UserArticleListDataCatche { get; set; }
        /// <summary>
        /// 用户编辑记录列表缓存
        /// </summary>
        public IPageModelCatche<PagedResultDto<ExaminedNormalListModel>> UserExaminesDataCatche { get; set; }
        /// <summary>
        /// 用户收藏夹列表缓存
        /// </summary>
        public IPageModelCatche<FavoriteFoldersViewModel> UserFavoriteFoldersDataCatche { get; set; }
        /// <summary>
        /// 用户收藏对象列表缓存
        /// </summary>
        public IPageModelCatche<PagedResultDto<FavoriteObjectAloneViewModel>> UserFavoriteObjectsDataCatche { get; set; }
        /// <summary>
        /// 游戏发售时间列表缓存
        /// </summary>
        public IPageModelCatche<List<EntryInforTipViewModel>> PublishGameTimesDataCatche { get; set; }

        /// <summary>
        /// 用户待审核对象列表缓存
        /// </summary>
        public UserPendingDataCacheModel UserPendingDataCatche { get; set; } = new UserPendingDataCacheModel();
        /// <summary>
        /// 文档缓存
        /// </summary>
        public List<DocumentViewModel> DocumentsCatche { get; set; } = new List<DocumentViewModel>();
        /// <summary>
        /// 免费游戏列表
        /// </summary>
        public List<EntryInforTipViewModel> FreeGamesCatche { get; set; } = new List<EntryInforTipViewModel>();

        /// <summary>
        /// 广场抽奖列表缓存
        /// </summary>
        public List<LotteryCardViewModel> LotteryCardsCatche { get; set; } = new List<LotteryCardViewModel>();
        /// <summary>
        /// 广场投票列表缓存
        /// </summary>
        public List<VoteCardViewModel> VoteCardsCatche { get; set; } = new List<VoteCardViewModel>();

        /// <summary>
        /// 文章主页 随机文章
        /// </summary>
        public List<RandomTagModel> RandomTagsCatche { get; set; } = new List<RandomTagModel>();

        /// <summary>
        /// 文章主页 随机文章
        /// </summary>
        public List<ArticleInforTipViewModel> RandomArticlesCatche { get; set; } = new List<ArticleInforTipViewModel>();
        /// <summary>
        /// 文章主页 周报列表
        /// </summary>
        public List<ArticleInforTipViewModel> WeeklyNewsOverviewCatche { get; set; } = new List<ArticleInforTipViewModel>();
        /// <summary>
        /// 文章主页 游戏评测预览
        /// </summary>
        public List<GameEvaluationsModel> GameEvaluationsCatche { get; set; } = new List<GameEvaluationsModel>();

        /// <summary>
        /// 词条主页 CG预览列表
        /// </summary>
        public List<GameCGModel> GameCGsCatche { get; set; } = new List<GameCGModel>();
        /// <summary>
        /// 词条主页 角色预览列表
        /// </summary>
        public List<GameRoleModel> GameRolesCatche { get; set; } = new List<GameRoleModel>();
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
        /// 主页缓存
        /// </summary>
        public List<KeyValuePair<List<CnGalWebSite.Shared.AppComponent.Normal.Cards.MainImageCardModel>, string>> HomeListCards { get; set; } = new List<KeyValuePair<List<AppComponent.Normal.Cards.MainImageCardModel>, string>>();
        /// <summary>
        /// 搜索页面缓存
        /// </summary>
        public IPageModelCatche<SearchViewModel> SearchViewCatche { get; set; }

        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;

        public DataCatcheService(HttpClient httpClient, IPageModelCatche<EntryIndexViewModel> entryIndexPageCatche, IPageModelCatche<ArticleViewModel> articleIndexPageCatche, IPageModelCatche<VoteViewModel> voteIndexPageCatche,
        IPageModelCatche<PeripheryViewModel> peripheryIndexPageCatche, IPageModelCatche<TagIndexViewModel> tagIndexPageCatche, IPageModelCatche<List<HomeNewsAloneViewModel>> homePageNewsCatche,
        IPageModelCatche<List<CarouselViewModel>> homePageCarouselsCatche, IPageModelCatche<ExaminesOverviewViewModel> examinesOverviewCatche, NavigationManager navigationManager,
        IPageModelCatche<LotteryViewModel> lotteryIndexPageCatche,
        IPageModelCatche<ArticleContrastEditRecordViewModel> articleContrastEditRecordViewCatche,
        IPageModelCatche<PeripheryContrastEditRecordViewModel> peripheryContrastEditRecordViewCatche,
        IPageModelCatche<TagContrastEditRecordViewModel> tagContrastEditRecordViewCatche,
        IPageModelCatche<EntryContrastEditRecordViewModel> entryContrastEditRecordViewCatche,
        IPageModelCatche<SearchViewModel> searchViewCatche,
        IPageModelCatche<LineChartModel> lineChartDataCatche,
        IPageModelCatche<CommentCacheModel> commentDataCatche,
        IPageModelCatche<PersonalSpaceViewModel> personalSpaceDataCatche,
        IPageModelCatche<List<GameRecordViewModel>> userGameRecordDataCatche,
        IPageModelCatche<List<SteamUserInfor>> userSteamInforDataCatche,
        IPageModelCatche<UserArticleListModel> userArticleListDataCatche,
        IPageModelCatche<PagedResultDto<ExaminedNormalListModel>> userExaminesDataCatche,
        IPageModelCatche<FavoriteFoldersViewModel> userFavoriteFoldersDataCatche,
        IPageModelCatche<PagedResultDto<FavoriteObjectAloneViewModel>> userFavoriteObjectsDataCatche,
        IPageModelCatche<PlayedGameOverviewModel> playedGameOverviewDataCatche,
        IPageModelCatche<List<EntryInforTipViewModel>> publishGameTimesDataCatche)
        {
            _httpClient = httpClient;
            (EntryIndexPageCatche = entryIndexPageCatche).Init(ToolHelper.WebApiPath + "api/entries/GetEntryView/");
            (ArticleIndexPageCatche = articleIndexPageCatche).Init(ToolHelper.WebApiPath + "api/articles/GetArticleView/");
            (PeripheryIndexPageCatche = peripheryIndexPageCatche).Init(ToolHelper.WebApiPath + "api/peripheries/GetPeripheryView/");
            (VoteIndexPageCatche = voteIndexPageCatche).Init(ToolHelper.WebApiPath + "api/votes/GetVoteView/");
            (LotteryIndexPageCatche = lotteryIndexPageCatche).Init(ToolHelper.WebApiPath + "api/lotteries/GetLotteryView/");
            (TagIndexPageCatche = tagIndexPageCatche).Init(ToolHelper.WebApiPath + "api/tags/gettag/");
            (ExaminesOverviewCatche = examinesOverviewCatche).Init(ToolHelper.WebApiPath + "api/examines/GetExaminesOverview/");
            (EntryContrastEditRecordViewCatche = entryContrastEditRecordViewCatche).Init(ToolHelper.WebApiPath + "api/entries/GetContrastEditRecordViews/");
            (ArticleContrastEditRecordViewCatche = articleContrastEditRecordViewCatche).Init(ToolHelper.WebApiPath + "api/articles/GetContrastEditRecordViews/");
            (PeripheryContrastEditRecordViewCatche = peripheryContrastEditRecordViewCatche).Init(ToolHelper.WebApiPath + "api/peripheries/GetContrastEditRecordViews/");
            (TagContrastEditRecordViewCatche = tagContrastEditRecordViewCatche).Init(ToolHelper.WebApiPath + "api/tags/GetContrastEditRecordViews/");
            (PlayedGameOverviewDataCatche = playedGameOverviewDataCatche).Init(ToolHelper.WebApiPath + "api/playedgame/GetPlayedGameOverview/");
            (CommentDataCatche = commentDataCatche).Init(ToolHelper.WebApiPath + "api/comments/GetComments/");
            (PersonalSpaceDataCatche = personalSpaceDataCatche).Init(ToolHelper.WebApiPath + "api/space/getuserview/");
            (UserGameRecordDataCatche = userGameRecordDataCatche).Init(ToolHelper.WebApiPath + "api/playedgame/GetUserGameRecords/");
            (UserSteamInforDataCatche = userSteamInforDataCatche).Init(ToolHelper.WebApiPath + "api/steam/GetUserSteamInfor/");
            (UserArticleListDataCatche = userArticleListDataCatche).Init(ToolHelper.WebApiPath + "api/space/GetUserArticles/");
            (UserExaminesDataCatche = userExaminesDataCatche).Init(ToolHelper.WebApiPath + "api/space/GetUserEditRecord");
            (UserFavoriteFoldersDataCatche = userFavoriteFoldersDataCatche).Init(ToolHelper.WebApiPath + "api/favorites/GetUserFavoriteFolders/");
            (UserFavoriteObjectsDataCatche = userFavoriteObjectsDataCatche).Init(ToolHelper.WebApiPath + "api/favorites/GetUserFavoriteObjectList");
            (PublishGameTimesDataCatche = publishGameTimesDataCatche).Init(ToolHelper.WebApiPath + "api/entries/GetPublishGamesByTime");

            (LineChartDataCatche = lineChartDataCatche).Init("");
            HomePageNewsCatche = homePageNewsCatche;
            HomePageCarouselsCatche = homePageCarouselsCatche;
            SearchViewCatche = searchViewCatche;
            _navigationManager = navigationManager;
        }

        public async Task<List<CnGalWebSite.Shared.AppComponent.Normal.Cards.MainImageCardModel>> GetHomePageListCardMode(string apiUrl, string type, int maxCount, bool isRefresh)
        {
            //查找是否存在缓存
            if (isRefresh == false)
            {
                var model = HomeListCards.FirstOrDefault(s => s.Value == apiUrl);
                if (model.Key != null)
                {
                    return model.Key;
                }
            }
            else
            {
                HomeListCards.RemoveAll(s => s.Value == apiUrl);
            }

            //获取新数据
            var items = new List<CnGalWebSite.Shared.AppComponent.Normal.Cards.MainImageCardModel>();
            try
            {
                var model = await _httpClient.GetFromJsonAsync<List<EntryHomeAloneViewModel>>(ToolHelper.WebApiPath + apiUrl);
                //转换数据
                foreach (var item in model.Take(maxCount))
                {
                    items.Add(new AppComponent.Normal.Cards.MainImageCardModel
                    {
                        CommentCount = item.CommentCount,
                        Image = item.Image,
                        Name = item.DisPlayName,
                        ReadCount = item.ReadCount,
                        Url = string.IsNullOrWhiteSpace(type) ? item.DisPlayValue : (_navigationManager.BaseUri+type + "/index/" + item.Id),
                        IsOutlink= string.IsNullOrWhiteSpace(type)
                    });
                }
                HomeListCards.Add(new KeyValuePair<List<CnGalWebSite.Shared.AppComponent.Normal.Cards.MainImageCardModel>, string>(items, apiUrl));
                return items;
            }
            catch (Exception)
            {
                return new List<AppComponent.Normal.Cards.MainImageCardModel>();
            }
        }


        public void OnRefreshRequsted(EventArgs e)
        {
            EventHandler<EventArgs> handler = RefreshRequsted;
            handler?.Invoke(this, e);
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
            LoginKey = null;
            ThirdPartyLoginTempModel = null;
            UserInfor = new UserInforViewModel { Ranks = new List<RankViewModel>() };
        }
    }
}
