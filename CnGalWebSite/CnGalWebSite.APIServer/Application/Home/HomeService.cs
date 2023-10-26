
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Home;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Carousel = CnGalWebSite.DataModel.Model.Carousel;

namespace CnGalWebSite.APIServer.Application.Home
{
    public class HomeService : IHomeService
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Carousel, int> _carouselRepository;
        private readonly IRepository<FriendLink, int> _friendLinkRepository;
        private readonly IRepository<Video, int> _videoRepository;
        private readonly IAppHelper _appHelper;
        private readonly IArticleService _articleService;

        public HomeService(IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Article, long> articleRepository, IRepository<Video, int> videoRepository,
        IArticleService articleService,
        IRepository<Carousel, int> carouselRepository, IRepository<FriendLink, int> friendLinkRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _carouselRepository = carouselRepository;
            _friendLinkRepository = friendLinkRepository;
            _articleService = articleService;
            _videoRepository = videoRepository;
        }

        public async Task<List<PublishedGameItemModel>> ListPublishedGames()
        {
            var model = new List<PublishedGameItemModel>();

            //获取近期新作
            var tempDateTimeNow = DateTime.Now.ToCstTime().Date;
            var entry_result3 = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Tags)
                .Where(s => s.Type == EntryType.Game && s.PubulishTime != null && s.PubulishTime.Value.Date <= tempDateTimeNow && s.Name != null && s.Name != "" && s.IsHidden != true)
                .OrderByDescending(s => s.PubulishTime)
                .Take(16)
                .ToListAsync();
            if (entry_result3 != null)
            {
                foreach (var item in entry_result3)
                {
                    model.Add(new PublishedGameItemModel
                    {
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                        Name = item.DisplayName,
                        Url = "entries/index/" + item.Id,
                        BriefIntroduction = item.BriefIntroduction,
                        Tags = item.Tags.Where(s => s.Name.Contains("字幕") == false && s.Name.Contains("语音") == false && s.Name.Contains("界面") == false).Select(s => s.Name).ToList()
                    });
                }
            }

            return model;
        }

        public async Task<List<RecentlyEditedGameItemModel>> ListRecentlyEditedGames()
        {
            var model = new List<RecentlyEditedGameItemModel>();
            var tempDateTimeNow = DateTime.Now.ToCstTime().Date;

            var entryIds = await _entryRepository.GetAll().AsNoTracking().OrderByDescending(s => s.PubulishTime)
                    .Where(s => s.Type == EntryType.Game && s.PubulishTime != null && s.PubulishTime.Value.Date < tempDateTimeNow && s.Name != null && s.Name != "" && s.IsHidden != true).Select(s => s.Id).Take(6).ToListAsync();
            entryIds.AddRange(await _entryRepository.GetAll().AsNoTracking()
                    .Where(s => s.Type == EntryType.Game && s.Name != null && s.Name != "" && s.IsHidden != true && s.PubulishTime != null && s.PubulishTime.Value.Date > tempDateTimeNow)
                    .OrderBy(s => s.PubulishTime).Select(s => s.Id).Take(6).ToListAsync());


            //获取近期编辑
            var entry_result2 = await _entryRepository.GetAll().OrderByDescending(s => s.LastEditTime).AsNoTracking()
                .Where(s => s.Type == EntryType.Game && s.Name != null && s.Name != "" && s.IsHidden != true && entryIds.Contains(s.Id) == false).Take(6).ToListAsync();
            if (entry_result2 != null)
            {
                foreach (var item in entry_result2)
                {
                    model.Add(new RecentlyEditedGameItemModel
                    {
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                        Name = item.DisplayName,
                        Url = "entries/index/" + item.Id,
                        PublishTime = item.LastEditTime.ToTimeFromNowString()
                    });
                }
            }
            return model;
        }

        public async Task<List<UpcomingGameItemModel>> ListUpcomingGames()
        {
            var model = new List<UpcomingGameItemModel>();
            var dateTime = DateTime.Now.ToCstTime().Date;
            //获取即将发售
            var entry_result1 = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Releases)
                .Where(s => s.Type == EntryType.Game && s.Name != null && s.Name != "" && s.IsHidden != true && s.PubulishTime != null && s.PubulishTime.Value.Date > dateTime)
                .OrderBy(s => s.PubulishTime).Take(16).ToListAsync();
            if (entry_result1 != null)
            {
                foreach (var item in entry_result1)
                {
                    var temp = new UpcomingGameItemModel
                    {
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                        Name = item.DisplayName,
                        Url = "entries/index/" + item.Id,
                        BriefIntroduction = item.BriefIntroduction,
                    };

                    if (item.PubulishTime != null)
                    {
                        var note = item.Releases.OrderBy(s => s.Time).FirstOrDefault(s => s.Type == GameReleaseType.Official && string.IsNullOrWhiteSpace(s.TimeNote) == false)?.TimeNote;
                        if (string.IsNullOrWhiteSpace(note))
                        {
                            temp.PublishTime = item.PubulishTime.Value.AddHours(8).ToString("yyyy年M月d日");
                        }
                        else
                        {
                            temp.PublishTime = note;
                        }
                    }

                    model.Add(temp);
                }
            }

            return model;

        }

        public async Task<List<FriendLinkItemModel>> ListFriendLinks()
        {
            var model = new List<FriendLinkItemModel>();

            //获取友情置顶词条 根据优先级排序
            var entry_result4 = await _friendLinkRepository.GetAll().AsNoTracking().OrderByDescending(s => s.Priority)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (entry_result4 != null)
            {
                foreach (var item in entry_result4)
                {
                    model.Add(new FriendLinkItemModel
                    {
                        Image = _appHelper.GetImagePath(item.Image, "app.png"),
                        Url = item.Link,
                        Name = item.Name,
                    });
                }

            }
            return model;
        }

        public async Task<List<AnnouncementItemModel>> ListAnnouncements()
        {
            var model = new List<AnnouncementItemModel>();

            //获取公告
            var articles = await _articleRepository.GetAll().AsNoTracking()
                .Where(s => s.IsHidden != true).AsNoTracking().OrderByDescending(s => s.Priority).ThenByDescending(s => s.PubishTime)
                .Where(s => s.Type == ArticleType.Notice && s.IsHidden != true).Take(6).ToListAsync();
            foreach (var item in articles)
            {
                model.Add(new AnnouncementItemModel
                {
                    Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png"),
                    Name = item.DisplayName,
                    Url = "articles/index/" + item.Id,
                    Priority = item.Priority,
                    BriefIntroduction = item.BriefIntroduction,
                });
            }
            return model;

        }

        public async Task<List<LatestArticleItemModel>> ListLatestArticles()
        {
            var model = new List<LatestArticleItemModel>();

            //获取近期发布的文章
            var article_result2 = await _articleRepository.GetAll().AsNoTracking()
                .Include(s => s.CreateUser)
                .Where(s => s.Priority >= 0)
                .Where(s => s.IsHidden != true && s.Type != ArticleType.Notice && s.Type != ArticleType.News).AsNoTracking().OrderByDescending(s => s.PubishTime).ThenByDescending(s => s.Id)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (article_result2 != null)
            {
                foreach (var item in article_result2)
                {
                    var temp = new LatestArticleItemModel
                    {
                        Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png"),
                        Name = item.DisplayName,
                        Url = "articles/index/" + item.Id,
                        BriefIntroduction = item.BriefIntroduction,
                        OriginalAuthor = item.OriginalAuthor,
                        UserName = item.CreateUser.UserName,
                        UserImage = _appHelper.GetImagePath(item.CreateUser.PhotoPath, "user.png"),
                        PublishTime = item.PubishTime.ToTimeFromNowString()
                    };
                    model.Add(temp);

                    if (temp.BriefIntroduction.Contains("原作者："))
                    {
                        var link = temp.BriefIntroduction.MidStrEx("原作者：", "游戏链接：");
                        var game = temp.BriefIntroduction.MidStrEx("游戏链接：", "》");

                        if (string.IsNullOrWhiteSpace(game) == false && game.Length < 20)
                        {
                            temp.BriefIntroduction = temp.BriefIntroduction.Replace($"原作者：{link}游戏链接：{game}》", "");
                        }
                        else
                        {
                            temp.BriefIntroduction = temp.BriefIntroduction.Replace($"原作者：{link}游戏链接：", "");
                        }

                    }
                }
            }
            return model;

        }

        public async Task<List<LatestVideoItemModel>> ListLatestVideos()
        {
            var model = new List<LatestVideoItemModel>();

            //获取近期发布的视频

            var article_result2 = await _videoRepository.GetAll().AsNoTracking()
                .Include(s => s.CreateUser)
                .Where(s => s.IsHidden != true).AsNoTracking().OrderByDescending(s => s.PubishTime).ThenByDescending(s => s.Id)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (article_result2 != null)
            {
                foreach (var item in article_result2)
                {
                    model.Add(new LatestVideoItemModel
                    {
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                        Name = item.DisplayName,
                        Url = "videos/index/" + item.Id,
                        OriginalAuthor = string.IsNullOrWhiteSpace(item.OriginalAuthor) ? item.CreateUser.UserName : item.OriginalAuthor,
                        PublishTime = item.PubishTime.ToTimeFromNowString()

                    });
                }
            }
            return model;

        }

        public async Task<List<HomeNewsAloneViewModel>> GetHomeNewsViewAsync()
        {
            var model = new List<HomeNewsAloneViewModel>();

            //获取近期发布的文章
            var article_result2 = await _articleRepository.GetAll().Include(s => s.CreateUser).Where(s => s.IsHidden != true && s.Type == ArticleType.News).AsNoTracking()
                .Include(s => s.CreateUser)
                .Include(s => s.Entries)
                .OrderByDescending(s => s.RealNewsTime).ThenByDescending(s => s.PubishTime)
                .Where(s => s.Name != null && s.Name != "").Take(18).ToListAsync();
            if (article_result2 != null)
            {
                foreach (var item in article_result2)
                {
                    var infor = await _articleService.GetNewsModelAsync(item);
                    var temp = new HomeNewsAloneViewModel
                    {
                        ArticleId = item.Id,
                        Text = infor.Title,
                        Time = infor.HappenedTime,
                        Type = infor.NewsType ?? "动态",
                        GroupId = infor.GroupId,
                        Image = infor.Image,
                        Link = infor.Link,
                        Title = infor.GroupName,
                        UserId = infor.UserId,
                    };
                    model.Add(temp);
                }
            }
            return model;

        }

        public async Task<List<CarouselViewModel>> GetHomeCarouselsViewAsync()
        {
            var carouses = await _carouselRepository.GetAll().AsNoTracking().Where(s => s.Type == CarouselType.Home && s.Priority >= 0).OrderByDescending(s => s.Priority).ToListAsync();

            var model = new List<CarouselViewModel>();
            foreach (var item in carouses)
            {
                model.Add(new CarouselViewModel
                {
                    Image = _appHelper.GetImagePath(item.Image, ""),
                    Link = item.Link,
                    Note = item.Note,
                    Priority = item.Priority,
                    Type = item.Type,
                    Id = item.Id,
                });
            }

            return model;
        }
    }
}
