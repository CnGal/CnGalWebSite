using BootstrapBlazor.Components;
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
        private readonly IAppHelper _appHelper;
        private readonly IArticleService _articleService;

        public HomeService(IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Article, long> articleRepository,
            IArticleService articleService,
        IRepository<Carousel, int> carouselRepository, IRepository<FriendLink, int> friendLinkRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _carouselRepository = carouselRepository;
            _friendLinkRepository = friendLinkRepository;
            _articleService = articleService;
        }

        public async Task<List<MainImageCardModel>> GetHomeNewestGameViewAsync()
        {
            var model = new List<MainImageCardModel>();
            var dateTime = DateTime.Now.ToCstTime();
            //获取即将发售
            var entry_result1 = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => s.Type == EntryType.Game && s.Name != null && s.Name != "" && s.IsHidden != true && s.PubulishTime > dateTime)
                .OrderBy(s => s.PubulishTime).Take(12).ToListAsync();
            if (entry_result1 != null)
            {
                foreach (var item in entry_result1)
                {
                    model.Add(new MainImageCardModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                        Name = item.DisplayName ,
                        IsOutlink=false,
                        Url="entries/index/"+item.Id,
                        CommentCount = item.CommentCount,
                        ReadCount = item.ReaderCount,
                        // DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }
            }

            return model;
        }

        public async Task<List<MainImageCardModel>> GetHomeRecentEditViewAsync()
        {
            var model = new List<MainImageCardModel>();
            var tempDateTimeNow = DateTime.Now.ToCstTime();

            var entryIds = await _entryRepository.GetAll().AsNoTracking().OrderByDescending(s => s.PubulishTime)
                    .Where(s => s.Type == EntryType.Game && s.PubulishTime < tempDateTimeNow && s.Name != null && s.Name != "" && s.IsHidden != true).Select(s => s.Id).Take(12).ToListAsync();
            entryIds.AddRange(await _entryRepository.GetAll().AsNoTracking()
                    .Where(s => s.Type == EntryType.Game && s.Name != null && s.Name != "" && s.IsHidden != true && s.PubulishTime > tempDateTimeNow)
                    .OrderBy(s => s.PubulishTime).Select(s => s.Id).Take(12).ToListAsync());


            //获取近期编辑
            var entry_result2 = await _entryRepository.GetAll().OrderByDescending(s => s.LastEditTime).AsNoTracking()
                .Where(s => s.Type == EntryType.Game && s.Name != null && s.Name != "" && s.IsHidden != true && entryIds.Contains(s.Id) == false).Take(12).ToListAsync();
            if (entry_result2 != null)
            {
                foreach (var item in entry_result2)
                {
                    model.Add(new MainImageCardModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                        Name = item.DisplayName,
                        Url = "entries/index/" + item.Id,
                        CommentCount = item.CommentCount,
                        ReadCount = item.ReaderCount,
                        //  DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }
            }
            return model;
        }

        public async Task<List<MainImageCardModel>> GetHomeRecentIssuelGameViewAsync()
        {
            var model = new List<MainImageCardModel>();

            //获取近期新作
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            var entry_result3 = await _entryRepository.GetAll().AsNoTracking().OrderByDescending(s => s.PubulishTime)
                .Where(s => s.Type == EntryType.Game && s.PubulishTime < tempDateTimeNow && s.Name != null && s.Name != "" && s.IsHidden != true).Take(12).ToListAsync();
            if (entry_result3 != null)
            {
                foreach (var item in entry_result3)
                {
                    model.Add(new MainImageCardModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                        Name = item.DisplayName,
                        Url = "entries/index/" + item.Id,
                        CommentCount = item.CommentCount,
                        ReadCount = item.ReaderCount,
                        //  DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }
            }
            return model;

        }

        public async Task<List<MainImageCardModel>> GetHomeFriendLinksViewAsync()
        {
            var model = new List<MainImageCardModel>();

            //获取友情置顶词条 根据优先级排序
            var entry_result4 = await _friendLinkRepository.GetAll().AsNoTracking().OrderByDescending(s => s.Priority)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (entry_result4 != null)
            {
                foreach (var item in entry_result4)
                {
                    model.Add(new MainImageCardModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.Image, "app.png"),
                        Url = item.Link,
                        Name = item.Name,
                        IsOutlink=true,
                        CommentCount = -1,
                        ReadCount = -1
                        //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }

            }
            return model;
        }

        public async Task<List<MainImageCardModel>> GetHomeNoticesViewAsync()
        {
            var model = new List<MainImageCardModel>();

            //获取公告
            var articles = await _articleRepository.GetAll().Where(s => s.IsHidden != true).AsNoTracking().OrderByDescending(s => s.Priority).ThenByDescending(s => s.PubishTime)
                .Where(s => s.Type == ArticleType.Notice && s.IsHidden != true).Take(12).ToListAsync();
            foreach (var item in articles)
            {
                model.Add(new MainImageCardModel
                {
                    Id = item.Id,
                    Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png"),
                    Name = item.DisplayName ,
                    Url = "articles/index/" + item.Id,
                    CommentCount = item.CommentCount,
                    ReadCount = item.ReaderCount,
                    //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                });
            }
            return model;

        }

        public async Task<List<MainImageCardModel>> GetHomeArticlesViewAsync()
        {
            var model = new List<MainImageCardModel>();

            //获取近期发布的文章
            var article_result2 = await _articleRepository.GetAll().Where(s => s.IsHidden != true && s.Type != ArticleType.Notice && s.Type != ArticleType.News).AsNoTracking().OrderByDescending(s => s.PubishTime).ThenByDescending(s => s.Id)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (article_result2 != null)
            {
                foreach (var item in article_result2)
                {
                    model.Add(new MainImageCardModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png"),
                        Name = item.DisplayName,
                        Url = "articles/index/" + item.Id,
                        CommentCount = item.CommentCount,
                        ReadCount = item.ReaderCount,
                        //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
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
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
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
            var carouses = await _carouselRepository.GetAll().AsNoTracking().Where(s=>s.Type== CarouselType.Home).OrderByDescending(s => s.Priority).ToListAsync();

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
                });
            }

            return model;
        }
    }
}
