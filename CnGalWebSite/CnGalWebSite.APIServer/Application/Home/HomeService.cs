using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Home;
using System;
using System.Collections.Concurrent;
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

        public HomeService(IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Article, long> articleRepository, IRepository<Carousel, int> carouselRepository, IRepository<FriendLink, int> friendLinkRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _carouselRepository = carouselRepository;
            _friendLinkRepository = friendLinkRepository;
        }

        public async Task<List<EntryHomeAloneViewModel>> GetHomeNewestGameViewAsync()
        {
            var model = new List<EntryHomeAloneViewModel>();
            var dateTime = DateTime.Now.ToCstTime();
            //获取即将发售
            var entry_result1 = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => s.Type == EntryType.Game && s.Name != null && s.Name != "" && s.IsHidden != true && s.PubulishTime > dateTime)
                .OrderBy(s => s.PubulishTime).Take(12).ToListAsync();
            if (entry_result1 != null)
            {
                foreach (var item in entry_result1)
                {
                    model.Add(new EntryHomeAloneViewModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png",true),
                        DisPlayName = item.DisplayName ?? item.Name,
                        CommentCount = item.CommentCount,
                        ReadCount = item.ReaderCount,
                        // DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }
            }

            return model;
        }

        public async Task<List<EntryHomeAloneViewModel>> GetHomeRecentEditViewAsync()
        {
            var model = new List<EntryHomeAloneViewModel>();

            //获取近期编辑
            var entry_result2 = await _entryRepository.GetAll().OrderByDescending(s => s.LastEditTime).AsNoTracking()
                .Where(s => s.Type == EntryType.Game && s.Name != null && s.Name != "" && s.IsHidden != true).Take(12).ToListAsync();
            if (entry_result2 != null)
            {
                foreach (var item in entry_result2)
                {
                    model.Add(new EntryHomeAloneViewModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png", true),
                        DisPlayName = item.DisplayName ?? item.Name,
                        CommentCount = item.CommentCount,
                        ReadCount = item.ReaderCount,
                        //  DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }
            }
            return model;
        }

        public async Task<List<EntryHomeAloneViewModel>> GetHomeRecentIssuelGameViewAsync()
        {
            var model = new List<EntryHomeAloneViewModel>();

            //获取近期新作
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            var entry_result3 = await _entryRepository.GetAll().AsNoTracking().OrderByDescending(s => s.PubulishTime)
                .Where(s => s.Type == EntryType.Game && s.PubulishTime < tempDateTimeNow && s.Name != null && s.Name != "" && s.IsHidden != true).Take(12).ToListAsync();
            if (entry_result3 != null)
            {
                foreach (var item in entry_result3)
                {
                    model.Add(new EntryHomeAloneViewModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png", true),
                        DisPlayName = item.DisplayName ?? item.Name,
                        CommentCount = item.CommentCount,
                        ReadCount = item.ReaderCount,
                        //  DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }
            }
            return model;

        }

        public async Task<List<EntryHomeAloneViewModel>> GetHomeFriendEntriesViewAsync()
        {
            var model = new List<EntryHomeAloneViewModel>();

            //获取友情置顶词条 根据优先级排序
            var entry_result4 = await _entryRepository.GetAll().AsNoTracking().OrderByDescending(s => s.Priority)
                .Where(s => (s.Type == EntryType.Game || s.Type == EntryType.ProductionGroup) && s.Name != null && s.Name != "" && s.IsHidden != true).Take(12).ToListAsync();
            if (entry_result4 != null)
            {
                foreach (var item in entry_result4)
                {
                    model.Add(new EntryHomeAloneViewModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "app.png", true),
                        DisPlayName = item.DisplayName ?? item.Name,
                        CommentCount = item.CommentCount,
                        ReadCount = item.ReaderCount,
                        //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }

            }

            return model;
        }

        public async Task<List<EntryHomeAloneViewModel>> GetHomeFriendLinksViewAsync()
        {
            var model = new List<EntryHomeAloneViewModel>();

            //获取友情置顶词条 根据优先级排序
            var entry_result4 = await _friendLinkRepository.GetAll().AsNoTracking().OrderByDescending(s => s.Priority)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (entry_result4 != null)
            {
                foreach (var item in entry_result4)
                {
                    model.Add(new EntryHomeAloneViewModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.Image, "app.png", true),
                        DisPlayValue = item.Link,
                        DisPlayName = item.Name,
                        CommentCount=-1,
                        ReadCount=-1
                        //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }

            }
            return model;
        }

        public async Task<List<EntryHomeAloneViewModel>> GetHomeNoticesViewAsync()
        {
            var model = new List<EntryHomeAloneViewModel>();

            //获取公告
            var articles = await _articleRepository.GetAll().Where(s => s.IsHidden != true).AsNoTracking().OrderByDescending(s => s.Priority).ThenByDescending(s => s.PubishTime)
                .Where(s => s.Type == ArticleType.Notice && s.IsHidden != true).Take(12).ToListAsync();
            foreach (var item in articles)
            {
                model.Add(new EntryHomeAloneViewModel
                {
                    Id = item.Id,
                    Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png", true),
                    DisPlayName = item.DisplayName ?? item.Name,
                    CommentCount = item.CommentCount,
                    ReadCount = item.ReaderCount,
                    //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                });
            }
            return model;

        }

        public async Task<List<EntryHomeAloneViewModel>> GetHomeArticlesViewAsync()
        {
            var model = new List<EntryHomeAloneViewModel>();

            //获取近期发布的文章
            var article_result2 = await _articleRepository.GetAll().Where(s => s.IsHidden != true && s.Type != ArticleType.Notice && s.Type != ArticleType.News).AsNoTracking().OrderByDescending(s => s.PubishTime).ThenByDescending(s => s.Id)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (article_result2 != null)
            {
                foreach (var item in article_result2)
                {
                    model.Add(new EntryHomeAloneViewModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png", true),
                        DisPlayName = item.DisplayName ?? item.Name,
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
            var article_result2 = await _articleRepository.GetAll().Include(s => s.CreateUser).Where(s => s.IsHidden != true && s.Type == ArticleType.News).Include(s => s.Relevances).AsNoTracking().OrderByDescending(s => s.RealNewsTime).ThenByDescending(s => s.PubishTime)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (article_result2 != null)
            {
                foreach (var item in article_result2)
                {
                    var temp = new HomeNewsAloneViewModel
                    {
                        ArticleId = item.Id,
                        Text = item.DisplayName ?? item.Name,
                        Time = item.RealNewsTime ?? item.PubishTime,
                        Type = item.NewsType ?? "动态",
                    };

                    var infor = item.Relevances.FirstOrDefault(s => s.Modifier == "制作组");
                    if(infor == null)
                    {
                        infor = item.Relevances.FirstOrDefault(s => s.Modifier == "STAFF");
                        if (infor == null)
                        {
                            infor = item.Relevances.FirstOrDefault(s => s.Modifier == "游戏");
                            if (infor == null)
                            {
                                infor = item.Relevances.FirstOrDefault(s => s.Modifier == "角色");
                            }
                        }
                    }


                    if (infor != null)
                    {
                        var group = await _entryRepository.FirstOrDefaultAsync(s => s.Name == infor.DisplayName);
                        if (group != null)
                        {
                            temp.Image = string.IsNullOrWhiteSpace(group.Thumbnail) ? _appHelper.GetImagePath(item.CreateUser.PhotoPath, "user.png") : _appHelper.GetImagePath(group.Thumbnail, "user.png");
                            temp.Title = group.DisplayName ?? group.Name;
                            temp.GroupId = group.Id;
                        }
                        else
                        {
                            temp.Image = _appHelper.GetImagePath(item.CreateUser.PhotoPath, "user.png");
                            temp.Title = item.CreateUser.UserName;
                            temp.UserId = item.CreateUser.Id;
                        }
                    }
                    else
                    {
                        temp.Image = _appHelper.GetImagePath(item.CreateUser.PhotoPath, "user.png");
                        temp.Title = item.CreateUser.UserName;
                        temp.UserId = item.CreateUser.Id;
                    }

                    temp.Link = item.OriginalLink;
                    if(temp.Title=="搬运姬"&&string.IsNullOrWhiteSpace(item.OriginalAuthor)==false)
                    {
                        temp.Title = item.OriginalAuthor;
                    }

                    model.Add(temp);
                }
            }
            return model;

        }

        public async Task<List<Carousel>> GetHomeCarouselsViewAsync()
        {
            var model = await _carouselRepository.GetAll().AsNoTracking().OrderByDescending(s => s.Priority).ToListAsync();
            foreach (var item in model)
            {
                item.Image = _appHelper.GetImagePath(item.Image, "");
            }

            return model;
        }
    }
}
