using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.Helper.Extensions;
using Markdig;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.News
{


    public class NewsService : INewsService
    {
        private readonly IConfiguration _configuration;
        private readonly IRSSHelper _rssHelper;
        private readonly IAppHelper _appHelper;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<WeiboUserInfor, long> _weiboUserInforRepository;
        private readonly IRepository<GameNews, long> _gameNewsRepository;
        private readonly IRepository<WeeklyNews, long> _weeklyNewsRepository;
        private readonly IExamineService _examineService;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<GameNews>, string, SortOrder, IEnumerable<GameNews>>> SortLambdaCacheGameNews = new();
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<WeeklyNews>, string, SortOrder, IEnumerable<WeeklyNews>>> SortLambdaCacheWeeklyNews = new();


        public NewsService(IConfiguration configuration, IRSSHelper rssHelper, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFileService fileService,
        IRepository<WeiboUserInfor, long> weiboUserInforRepository, IRepository<GameNews, long> gameNewsRepository, IExamineService examineService,
             UserManager<ApplicationUser> userManager, IRepository<Article, long> articleRepository, IRepository<WeeklyNews, long> weeklyNewsRepository)
        {
            _configuration = configuration;
            _rssHelper = rssHelper;
            _appHelper = appHelper;
            _entryRepository = entryRepository;
            _weiboUserInforRepository = weiboUserInforRepository;
            _gameNewsRepository = gameNewsRepository;
            _examineService = examineService;
            _userManager = userManager;
            _articleRepository = articleRepository;
            _weeklyNewsRepository = weeklyNewsRepository;
            _fileService = fileService;
        }


        public Task<QueryData<ListGameNewAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListGameNewAloneModel searchModel)
        {
            IEnumerable<GameNews> items = _gameNewsRepository.GetAll().AsNoTracking().Where(s => s.Title != "已删除");
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Author))
            {
                items = items.Where(item => item.Author?.Contains(searchModel.Author, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Title))
            {
                items = items.Where(item => item.MainPage?.Contains(searchModel.Title, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.BriefIntroduction))
            {
                items = items.Where(item => item.BriefIntroduction?.Contains(searchModel.BriefIntroduction, StringComparison.OrdinalIgnoreCase) ?? false);
            }


            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Author?.Contains(options.SearchText) ?? false)
                || (item.Title?.Contains(options.SearchText) ?? false)
                || (item.BriefIntroduction?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheGameNews.GetOrAdd(typeof(GameNews), key => LambdaExtensions.GetSortLambda<GameNews>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListGameNewAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListGameNewAloneModel
                {
                    Id = item.Id,
                    State = item.State,
                    Author = item.Author,
                    BriefIntroduction = item.BriefIntroduction,
                    PublishTime = item.PublishTime,
                    Title = item.Title,
                    ArticleId = item.ArticleId ?? 0,
                });
            }

            return Task.FromResult(new QueryData<ListGameNewAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public Task<QueryData<ListWeeklyNewAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListWeeklyNewAloneModel searchModel)
        {
            IEnumerable<WeeklyNews> items = _weeklyNewsRepository.GetAll().AsNoTracking();
            // 处理高级搜索

            if (!string.IsNullOrWhiteSpace(searchModel.Title))
            {
                items = items.Where(item => item.MainPage?.Contains(searchModel.Title, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.BriefIntroduction))
            {
                items = items.Where(item => item.BriefIntroduction?.Contains(searchModel.BriefIntroduction, StringComparison.OrdinalIgnoreCase) ?? false);
            }


            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Title?.Contains(options.SearchText) ?? false)
                || (item.BriefIntroduction?.Contains(options.SearchText) ?? false));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheWeeklyNews.GetOrAdd(typeof(WeeklyNews), key => LambdaExtensions.GetSortLambda<WeeklyNews>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListWeeklyNewAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListWeeklyNewAloneModel
                {
                    Id = item.Id,
                    State = item.State,
                    BriefIntroduction = item.BriefIntroduction,
                    PublishTime = item.PublishTime,
                    Title = item.Title,
                    CreateTime = item.CreateTime,
                    ArticleId = item.ArticleId ?? 0,
                });
            }

            return Task.FromResult(new QueryData<ListWeeklyNewAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        /// <summary>
        /// 获取最新动态
        /// </summary>
        /// <returns></returns>
        public async Task UpdateNewestGameNews()
        {
            //查找最新的RSS源的时间

            var time = await _gameNewsRepository.GetAll().AnyAsync() ?
                await _gameNewsRepository.GetAll().Include(s => s.RSS).Where(s => s.RSS.Type == OriginalRSSType.Weibo).MaxAsync(s => s.RSS.PublishTime) : DateTime.MinValue;
            var weiboes = await _rssHelper.GetOriginalWeibo(long.Parse(_configuration["RSSWeiboUserId"]), time);

            //获取周报
            var weekly = await _weeklyNewsRepository.GetAll().Include(s => s.News).OrderByDescending(s => s.CreateTime).FirstOrDefaultAsync();
            if (weekly == null || weekly.CreateTime.IsInSameWeek(DateTime.Now.ToCstTime()) == false)
            {
                weekly = await GenerateNewestWeeklyNews();
            }

            if (weiboes.Count == 0)
            {
                return;
            }
            //处理原始数据
            foreach (var item in weiboes)
            {
                try
                {
                    var temp = await ProcessingOriginalRSS(item);

                    temp = await _gameNewsRepository.InsertAsync(temp);


                    //判断是否需要立即发表
                    if (temp.State == GameNewsState.Publish)
                    {
                        try
                        {
                            await PublishNews(temp);

                            weekly.News.Add(temp);
                        }
                        catch
                        {
                            temp.State = GameNewsState.Edit;
                            await _gameNewsRepository.UpdateAsync(temp);
                        }

                    }
                }
                catch
                {

                }

            }

            await _weeklyNewsRepository.UpdateAsync(weekly);

        }

        /// <summary>
        /// 添加自定义微博动态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task AddGameMewsFromWeibo(long id, string keyword)
        {
            var item = await _rssHelper.GetOriginalWeibo(id, keyword);
            if (item == null)
            {
                throw new Exception("无法获取该微博，请尝试添加自定义动态");
            }
            var temp = await ProcessingOriginalRSS(item);

            temp = await _gameNewsRepository.InsertAsync(temp);

            //获取周报
            var weekly = await _weeklyNewsRepository.GetAll().Include(s => s.News).OrderByDescending(s => s.CreateTime).FirstOrDefaultAsync();
            if (weekly == null || weekly.CreateTime.IsInSameWeek(DateTime.Now.ToCstTime()) == false)
            {
                weekly = await GenerateNewestWeeklyNews();
            }
            //判断是否需要立即发表
            if (temp.State == GameNewsState.Publish)
            {
                try
                {
                    await PublishNews(temp);

                    weekly.News.Add(temp);
                }
                catch
                {
                    temp.State = GameNewsState.Edit;
                    await _gameNewsRepository.UpdateAsync(temp);
                }

            }
        }

        /// <summary>
        /// 更新所有Staff制作组的微博信息缓存
        /// </summary>
        /// <returns></returns>
        public async Task UpdateWeiboUserInforCache()
        {
            //获取所有Staff制作组的名称和微博Id
            //检查是否以及缓存
            //没有缓存则缓存

            var currentUserIds = await _weiboUserInforRepository.GetAll().AsNoTracking().Select(s => s.EntryId).ToListAsync();

            var staffs = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Information)
                .Where(s => currentUserIds.Contains(s.Id) == false &&
                  s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.Information.Any())
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Information
                }).ToListAsync();

            var staffWeiboes = new List<KeyValuePair<string, long>>();

            for (var i = 0; i < staffs.Count; i++)
            {
                var staff = staffs[i];
                //寻找微博Id
                try
                {
                    var infor = staff.Information.FirstOrDefault(s => s.DisplayName == "微博");
                    if (infor != null)
                    {
                        var temp = infor.DisplayValue.Split('/');
                        if (temp.Length > 1)
                        {
                            staffWeiboes.Add(new KeyValuePair<string, long>(staff.Name, long.Parse(temp.Last())));
                        }
                    }
                }
                catch
                {

                }

            }

            foreach (var item in staffWeiboes)
            {
                try
                {
                    await AddWeiboUserInfor(item.Key, item.Value);
                }
                catch (Exception)
                {

                }

            }
        }

        /// <summary>
        /// 获取本周周报 不存在则新建
        /// </summary>
        /// <returns></returns>
        public async Task<WeeklyNews> GenerateNewestWeeklyNews()
        {
            //查找上一次周报时间

            var last = await _weeklyNewsRepository.GetAll().Include(s => s.News).OrderByDescending(s => s.CreateTime).FirstOrDefaultAsync();

            var isNeedCreate = false;
            if (last == null)
            {
                isNeedCreate = true;
            }
            else
            {

                isNeedCreate = !DateTime.Now.ToCstTime().IsInSameWeek(last.CreateTime);
            }

            if (isNeedCreate)
            {
                //发表旧周报
                if (last != null && last.State == GameNewsState.Edit)
                {
                    await PublishWeeklyNews(last);
                }

                //创建新周报
                last = await _weeklyNewsRepository.InsertAsync(new WeeklyNews
                {
                    CreateTime = DateTime.Now.ToCstTime(),
                    Type = ArticleType.News
                });

            }

            return last;
        }

        /// <summary>
        /// 发布动态
        /// </summary>
        /// <param name="gameNews"></param>
        /// <returns></returns>
        public async Task PublishNews(GameNews gameNews)
        {
            var article = await GameNewsToArticle(gameNews);

            //走批量导入的流程
            var admin = await _userManager.FindByIdAsync(article.CreateUserId);
            await _examineService.AddNewArticleExaminesAsync(article, admin, "自动生成动态");

            //查找文章
            var articleId = await _articleRepository.GetAll().Where(s => s.Name == article.Name).Select(s => s.Id).FirstOrDefaultAsync();

            gameNews.ArticleId = articleId;
            gameNews.State = GameNewsState.Publish;

            await _gameNewsRepository.UpdateAsync(gameNews);

            //添加到当天周报
            //获取周报
            var weekly = await _weeklyNewsRepository.GetAll().Include(s => s.News).OrderByDescending(s => s.CreateTime).FirstOrDefaultAsync();
            if (weekly == null || weekly.CreateTime.IsInSameWeek(DateTime.Now.ToCstTime()) == false)
            {
                weekly = await GenerateNewestWeeklyNews();
            }

            if (weekly.News.Any(s => s.Id == gameNews.Id) == false)
            {
                weekly.News.Add(gameNews);
                await _weeklyNewsRepository.UpdateAsync(weekly);
            }
        }

        /// <summary>
        /// 将动态转换成文章
        /// </summary>
        /// <param name="gameNews"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Article> GameNewsToArticle(GameNews gameNews)
        {
            var article = new Article
            {
                Name = gameNews.Title,
                DisplayName = gameNews.Title,
                OriginalAuthor = gameNews.Author ?? "CnGal",
                OriginalLink = gameNews.Link,
                MainPage = gameNews.MainPage,
                BriefIntroduction = gameNews.BriefIntroduction,
                MainPicture = gameNews.MainPicture,
                Type = gameNews.Type,
                NewsType = gameNews.NewsType ?? "动态",
                CreateUserId = _configuration["NewsAdminId"],
                PubishTime = gameNews.PublishTime,
                RealNewsTime = gameNews.PublishTime,
                CreateTime = DateTime.Now.ToCstTime(),
            };

            //检查是否合规
            if (string.IsNullOrWhiteSpace(article.Name))
            {
                throw new Exception("名称不能为空");
            }
            //不能重名
            if (await _articleRepository.CountAsync(s => s.Name == article.Name) > 0)
            {
                throw new Exception("文章名称『" + article.Name + "』重复，请尝试使用显示名称");
            }

            //添加关联信息
            if (gameNews.Entries.Any())
            {
                var entryNames = gameNews.Entries.Select(s => s.EntryName);
                var entries = await _entryRepository.GetAll().Where(s => entryNames.Contains(s.Name)).ToListAsync();

                foreach (var entry in entries)
                {
                    article.Entries.Add(entry);
                }
            }
            return article;
        }

        /// <summary>
        /// 发布周报
        /// </summary>
        /// <param name="weeklyNews"></param>
        /// <returns></returns>
        public async Task PublishWeeklyNews(WeeklyNews weeklyNews)
        {

            var article = await WeeklyNewsToArticle(weeklyNews);
            article.PubishTime = DateTime.Now.ToCstTime();

            //走批量导入的流程
            var admin = await _userManager.FindByIdAsync(article.CreateUserId);
            await _examineService.AddNewArticleExaminesAsync(article, admin, "自动生成周报");

            //查找文章
            var articleId = await _articleRepository.GetAll().Where(s => s.Name == article.Name).Select(s => s.Id).FirstOrDefaultAsync();

            weeklyNews.ArticleId = articleId;
            weeklyNews.PublishTime = DateTime.Now.ToCstTime();
            weeklyNews.State = GameNewsState.Publish;

            await _weeklyNewsRepository.UpdateAsync(weeklyNews);
        }

        /// <summary>
        /// 将周报转成文章
        /// </summary>
        /// <param name="weeklyNews"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Article> WeeklyNewsToArticle(WeeklyNews weeklyNews)
        {
            if (string.IsNullOrWhiteSpace(weeklyNews.Title))
            {
                weeklyNews.Title = GenerateWeeklyNewsTitle(weeklyNews);
            }
            if (string.IsNullOrWhiteSpace(weeklyNews.BriefIntroduction))
            {
                weeklyNews.BriefIntroduction = GenerateWeeklyNewsBriefIntroduction(weeklyNews);
            }
            if (string.IsNullOrWhiteSpace(weeklyNews.MainPicture))
            {
                weeklyNews.MainPicture = GenerateWeeklyNewsMainImage(weeklyNews);
            }
            var mainPage = GenerateRealWeeklyNewsMainPage(weeklyNews);

            var article = new Article
            {
                Name = weeklyNews.Title,
                DisplayName= weeklyNews.Title,
                MainPage = mainPage,
                BriefIntroduction = weeklyNews.BriefIntroduction,
                MainPicture = weeklyNews.MainPicture,
                Type = ArticleType.News,
                PubishTime = DateTime.Now.ToCstTime(),
                CreateTime = DateTime.Now.ToCstTime(),
                NewsType = "周报",
                CreateUserId = _configuration["NewsAdminId"]
            };



            //检查是否合规
            if (string.IsNullOrWhiteSpace(article.Name))
            {
                throw new Exception("名称不能为空");
            }
            //不能重名
            if (await _articleRepository.CountAsync(s => s.Name == article.Name) > 0)
            {
                throw new Exception("文章名称『" + article.Name + "』重复，请尝试使用显示名称");

            }

            return article;
        }

        /// <summary>
        /// 重置周报
        /// </summary>
        /// <param name="weeklyNews"></param>
        /// <returns></returns>
        public async Task<WeeklyNews> ResetWeeklyNews(WeeklyNews weeklyNews)
        {
            weeklyNews.News.Clear();

            //获取在这个星期的所有动态
            var news = await _gameNewsRepository.GetAll()
                .Where(s => s.State == GameNewsState.Publish && weeklyNews.CreateTime.AddDays(7) > s.PublishTime && weeklyNews.CreateTime.AddDays(-7) < s.PublishTime)
                .ToListAsync();

            news = news.Where(s => s.PublishTime.IsInSameWeek(weeklyNews.CreateTime)).ToList();
            weeklyNews.News.AddRange(news);
            weeklyNews.News = weeklyNews.News.OrderBy(s => s.PublishTime).ToList();

            weeklyNews.Title = GenerateWeeklyNewsTitle(weeklyNews);
            weeklyNews.BriefIntroduction = GenerateWeeklyNewsBriefIntroduction(weeklyNews);
            weeklyNews.MainPage = "";
            weeklyNews.MainPicture = GenerateWeeklyNewsMainImage(weeklyNews);


            return weeklyNews;
        }

        /// <summary>
        /// 添加微博用户缓存
        /// </summary>
        /// <param name="entryName"></param>
        /// <param name="weiboId"></param>
        /// <returns></returns>
        public async Task AddWeiboUserInfor(string entryName, long weiboId)
        {
            var user = await _weiboUserInforRepository.GetAll().FirstOrDefaultAsync(s => s.WeiboId == weiboId);
            user ??= await _rssHelper.GetWeiboUserInfor(weiboId);


            var entry = await _entryRepository.GetAll().Include(s => s.Information).FirstOrDefaultAsync(s => s.Name == entryName);


            //添加主图
            //if (string.IsNullOrWhiteSpace(entry.MainPicture))
            //{
            //    if (entry.Type == EntryType.Staff || entry.Type == EntryType.Role)
            //    {
            //        entry.MainPicture = user.Image;
            //    }
            //    else
            //    {
            //        entry.MainPicture = await _fileService.SaveImageAsync(user.Image, _configuration["NewsAdminId"], 460, 215);
            //    }
            //}
            if (string.IsNullOrWhiteSpace(entry.Thumbnail))
            {
                entry.Thumbnail = user.Image;
            }

            entry.Information.Remove(entry.Information.FirstOrDefault(s => s.DisplayName == "微博"));
            entry.Information.Add(new BasicEntryInformation
            {
                Modifier = "相关网站",
                DisplayName = "微博",
                DisplayValue = "https://weibo.com/u/" + weiboId,
            });

            user.EntryId = entry.Id;

            if (user.Id == 0)
            {
                await _weiboUserInforRepository.InsertAsync(user);
            }
            else
            {
                await _weiboUserInforRepository.UpdateAsync(user);
            }


            await _entryRepository.UpdateAsync(entry);
        }


        public string GenerateWeeklyNewsTitle(WeeklyNews weeklyNews)
        {
            return $"CnGal每周速报（{weeklyNews.CreateTime.Year}年第{WeekOfYear(weeklyNews.CreateTime)}周）";
        }

        public string GenerateWeeklyNewsBriefIntroduction(WeeklyNews weeklyNews)
        {
            var model = "";
            foreach (var item in weeklyNews.News)
            {
                model += "★ " + item.Author + " - " + item.Title + "\n";
            }

            return model;
        }

        public string GenerateWeeklyNewsMainImage(WeeklyNews weeklyNews)
        {
            foreach (var item in weeklyNews.News)
            {
                if (string.IsNullOrWhiteSpace(item.MainPicture) == false)
                {
                    return item.MainPicture;
                }
            }

            return "";
        }

        public string GenerateRealWeeklyNewsMainPage(WeeklyNews weeklyNews)
        {
            weeklyNews.News = weeklyNews.News.OrderBy(s => s.PublishTime).ToList();

            if (string.IsNullOrWhiteSpace(weeklyNews.MainPage))
            {
                weeklyNews.MainPage = "";
            }
            var strList = weeklyNews.MainPage.Split("[Foot]");
            var model = new StringBuilder($"{strList[0]}\n\n");

            //目录
            model.AppendLine("## 概览");
            foreach (var item in weeklyNews.News)
            {
                model.Append($"- **{item.Author}** - {item.Title}\n\n");
            }
            //正文
            model.AppendLine("## 正文");
            foreach (var item in weeklyNews.News)
            {
                //以UTC时间处理 但是发布后需要以 UTC+8:00 展示
                model.Append($"### {item.Author} - {item.Title}\n\n{item.BriefIntroduction}\n\n[原文链接 - {item.PublishTime.AddHours(8):yyyy/M/d HH:mm}]({item.Link})\n\n");
                if (string.IsNullOrWhiteSpace(item.MainPicture) == false)
                {
                    model.Append($"![{item.Title}]({item.MainPicture})\n\n");
                }
                model.Append("---------------\n\n");
            }

            if (strList.Count() > 1)
            {
                model.Append(strList[1]);
            }

            return model.ToString();
        }

        public async Task<GameNews> ProcessingOriginalRSS(OriginalRSS originalRSS)
        {
            var entries = await _entryRepository.GetAll().Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Name).ToListAsync();
            var users = await _weiboUserInforRepository.GetAll().Include(s => s.Entry).ToListAsync();

            return originalRSS.Type switch
            {
                OriginalRSSType.Weibo => await ProcessingMicroblog(originalRSS, entries, users),
                _ => null
            };
        }

        public async Task<GameNews> ProcessingMicroblog(OriginalRSS originalRSS, List<string> entries, List<WeiboUserInfor> users)
        {
            var authorString = GetMicroblogAuthor(originalRSS.Description);
            var model = new GameNews
            {
                Type = ArticleType.News,
                RSS = originalRSS,
                Title =(await GetMicroblogTitle(originalRSS.Title, originalRSS.Description, authorString, 15)).Trim().Replace("\n","").Replace("\r", ""),
                BriefIntroduction =await GetMicroblogTitle(originalRSS.Title, originalRSS.Description, authorString, 500),
                Author = authorString,
                MainPage =await GetMicroblogMainPage(originalRSS.Description, authorString),
                Link = originalRSS.Link,
                MainPicture = await GetMicroblogMainImage(await GetMicroblogMainPage(originalRSS.Description, authorString)),
                PublishTime = originalRSS.PublishTime,
                State = GameNewsState.Edit
            };
            //修正作者
            if (string.IsNullOrWhiteSpace(model.Author))
            {
                model.Author = originalRSS.Author;
            }
            //获取原文时间
            if (model.Title.Length > 5)
            {
                var originalInfor = await _rssHelper.CorrectOriginalWeiboInfor(model.Title[0..5], GetMicroblogOriginalId(originalRSS.Description));
                if (originalInfor == null)
                {
                    model.IsOriginal = false;
                }
                else
                {
                    //检查是否重复
                    if (await _gameNewsRepository.GetAll().Include(s => s.RSS).AnyAsync(s => (s.RSS.Link == model.Link || s.Link == model.Link) && s.Title != "已删除"))
                    {
                        throw new Exception("该微博动态已存在");
                    }

                    model.IsOriginal = true;
                    model.Link = originalInfor.Link;
                    model.PublishTime = originalInfor.PublishTime;
                }

            }

            //查找关联词条
            var relatedEntries = new List<string>();
            foreach (var item in entries)
            {
                if (model.MainPage.Contains(item))
                {
                    relatedEntries.Add(item);
                }
            }

            //查找作者
            var author = users.FirstOrDefault(s => s.WeiboName == model.Author);
            var isAuthor = author != null;
            if (author != null)
            {
                relatedEntries.Add(author.Entry.Name);
            }

            //过滤关联词条
            relatedEntries = ScreenRelatedEntry(relatedEntries);

            foreach (var item in relatedEntries)
            {
                model.Entries.Add(new GameNewsRelatedEntry
                {
                    EntryName = item
                });
            }

            //如果找到了关联词条 也找到了 作者 而且有主图 那么直接发布
            if (isAuthor && model.Entries.Count > 1 && string.IsNullOrWhiteSpace(model.MainPicture) == false && model.IsOriginal)
            {
                model.State = GameNewsState.Publish;
            }

            return model;
        }

        public List<string> ScreenRelatedEntry(List<string> entries)
        {
            //清除重复
            ToolHelper.Purge(ref entries);

            var result = new List<string>();
            var keyword = new List<string>
            {
                "Unity",
                "Steam",
                "平安夜"
            };
            foreach (var entry in entries)
            {
                if (long.TryParse(entry, out var temp) == false)
                {
                    if (entry.Length > 2)
                    {
                        if (keyword.Contains(entry) == false)
                        {
                            result.Add(entry);
                        }
                    }

                }
            }

            return result;
        }

        public async Task<string> GetMicroblogTitle(string title, string description, string author, int maxLength)
        {
            //分割出转发之前的文本
            var temp = title.Split(":&ensp;");
            if (true) //(temp.Length <= 1)
            {
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
                title = Markdown.ToPlainText(await GetMicroblogMainPage(description, author), pipeline);
                if (title.Split(": ").Length > 1)
                {
                    title = title.Split(": ")[1];
                }
            }
            else
            {
                title = temp[1];
            }

            //去除 图片 
            do
            {
                var midStr = ToolHelper.MidStrEx(title, "[", "]");
                title = title.Replace($"[{midStr}]", "");

            } while (string.IsNullOrWhiteSpace(ToolHelper.MidStrEx(title, "[", "]")) == false);
            //去除 话题
            do
            {
                var midStr = ToolHelper.MidStrEx(title, "#", "#");
                title = title.Replace($"#{midStr}#", "");

            } while (string.IsNullOrWhiteSpace(ToolHelper.MidStrEx(title, "#", "#")) == false);

            return _appHelper.GetStringAbbreviation(title, maxLength);
        }

        public string GetMicroblogAuthor(string description)
        {
            var temp = description.Split("- 转发");
            if (temp.Length > 1)
            {
                description = temp[1];
            }
            return ToolHelper.MidStrEx(description, "@", "</a>");
        }

        public long GetMicroblogOriginalId(string description)
        {
            var temp = ToolHelper.MidStrEx(description, "- 转发 <a href=\"https://weibo.com/", "\" target=\"_blank\">@");
            if (string.IsNullOrWhiteSpace(temp))
            {
                return 0;
            }

            if (long.TryParse(temp, out var id))
            {
                return id;
            }
            else
            {
                return 0;
            }
        }


        public async Task<string> GetMicroblogMainPage(string description, string author)
        {
            var temp = description.Split("- 转发");
            if (temp.Length > 1)
            {
                description = temp[1];
            }
            temp = description.Split("@" + author + "</a>: ");
            if (temp.Length > 1)
            {
                description = temp[1];
            }


            var converter = new ReverseMarkdown.Converter();

            var markdown= converter.Convert(description).Replace("\\[\\]", "[]");

            //处理视频
            if (markdown.Contains("<video controls=\"controls\""))
            {
                var videoHtml = markdown.MidStrEx("<video", "</video>");
                if (string.IsNullOrWhiteSpace(videoHtml) == false)
                {
                    //提取图片
                    var image = videoHtml.MidStrEx("poster=\"", "\"");
                    //提取链接
                    var link = videoHtml.MidStrEx("<a href=\"", "\"");
                    //替换
                    markdown = markdown.Replace($"<video{videoHtml}</video>", $"![]({await _fileService.SaveImageAsync( image, _configuration["NewsAdminId"])})\n视频无法显示，请前往[微博视频]({link})观看\n");
                }

            }

            return markdown;
        }

        public async Task<string> GetMicroblogMainImage(string description)
        {
            var links = description.GetImageLinks();
            return links.Any() ? await _fileService.SaveImageAsync(links.Last(), _configuration["NewsAdminId"], 460, 215) : "";
        }

        private int WeekOfYear(DateTime curDay)
        {

            var firstdayofweek = (int)DateTime.Parse(curDay.Year.ToString() + "-01-01T00:00:00.000000Z").ToUniversalTime().DayOfWeek;

            var days = curDay.DayOfYear;

            var daysOutOneWeek = days - (7 - firstdayofweek);

            if (daysOutOneWeek <= 0)

            {

                return 1;

            }
            else
            {

                var weeks = daysOutOneWeek / 7;

                if (daysOutOneWeek % 7 != 0)
                {
                    weeks++;
                }

                return weeks + 1;

            }
        }
    }
}
