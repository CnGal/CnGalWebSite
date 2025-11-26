
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.News;
using CnGalWebSite.DrawingBed.Helper.Services;
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
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IExamineService _examineService;
        private readonly IFileService _fileService;
        private readonly IFileUploadService _fileUploadService;

        public NewsService(IConfiguration configuration, IRSSHelper rssHelper, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFileService fileService, IRepository<ApplicationUser, string> userRepository,
        IRepository<WeiboUserInfor, long> weiboUserInforRepository, IRepository<GameNews, long> gameNewsRepository, IExamineService examineService, IFileUploadService fileUploadService,
        IRepository<Article, long> articleRepository, IRepository<WeeklyNews, long> weeklyNewsRepository)
        {
            _configuration = configuration;
            _rssHelper = rssHelper;
            _appHelper = appHelper;
            _entryRepository = entryRepository;
            _weiboUserInforRepository = weiboUserInforRepository;
            _gameNewsRepository = gameNewsRepository;
            _examineService = examineService;

            _articleRepository = articleRepository;
            _weeklyNewsRepository = weeklyNewsRepository;
            _fileService = fileService;
            _userRepository = userRepository;
            _fileUploadService = fileUploadService;
        }

        /// <summary>
        /// 获取最新动态
        /// </summary>
        /// <returns></returns>
        public async Task UpdateNewestGameNews()
        {
            //查找最新的RSS源的时间
            var weiboTime = await _gameNewsRepository.GetAll().AnyAsync(s => s.RSS.Type == OriginalRSSType.Weibo) ?
                await _gameNewsRepository.GetAll().Include(s => s.RSS).Where(s => s.RSS.Type == OriginalRSSType.Weibo).MaxAsync(s => s.RSS.PublishTime) : DateTime.MinValue;

            var bilibiliTime = await _gameNewsRepository.GetAll().AnyAsync(s => s.RSS.Type == OriginalRSSType.Bilibili) ?
                await _gameNewsRepository.GetAll().Include(s => s.RSS).Where(s => s.RSS.Type == OriginalRSSType.Bilibili).MaxAsync(s => s.RSS.PublishTime) : DateTime.MinValue;

            var heyBoxTime = await _gameNewsRepository.GetAll().AnyAsync(s => s.RSS.Type == OriginalRSSType.HeyBox) ?
                await _gameNewsRepository.GetAll().Include(s => s.RSS).Where(s => s.RSS.Type == OriginalRSSType.HeyBox).MaxAsync(s => s.RSS.PublishTime) : DateTime.MinValue;

            // 获取rss源
            var rss = await _rssHelper.GetOriginalWeibo(long.Parse(_configuration["RSSWeiboUserId"]), weiboTime);
            rss.AddRange(await _rssHelper.GetOriginalBilibili(long.Parse(_configuration["RSSBilibiliUserId"]), bilibiliTime));
            rss.AddRange(await _rssHelper.GetOriginalHeyBox(heyBoxTime));

            //var rss = await _rssHelper.GetOriginalBilibili(long.Parse(_configuration["RSSBilibiliUserId"]), time);

            //获取周报
            var weekly = await _weeklyNewsRepository.GetAll().Include(s => s.News).OrderByDescending(s => s.CreateTime).FirstOrDefaultAsync();
            if (weekly == null || weekly.CreateTime.IsInSameWeek(DateTime.Now.ToCstTime()) == false)
            {
                weekly = await GenerateNewestWeeklyNews();
            }

            if (rss.Count == 0)
            {
                return;
            }

            //处理原始数据
            foreach (var item in rss)
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
                catch(Exception ex)
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
            var admin = await _userRepository.FirstOrDefaultAsync(s => s.Id == article.CreateUserId);
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
            var admin = await _userRepository.FirstOrDefaultAsync(s => s.Id == article.CreateUserId);
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
                DisplayName = weeklyNews.Title,
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


            var entry = await _entryRepository.GetAll().Include(s => s.Outlinks).FirstOrDefaultAsync(s => s.Name == entryName);

            if (string.IsNullOrWhiteSpace(entry.Thumbnail))
            {
                entry.Thumbnail = user.Image;
            }
            if (entry.Outlinks.Any(s => s.Name == "微博") == false)
            {
                entry.Outlinks.Add(new Outlink
                {
                    Name = "微博",
                    Link = "https://weibo.com/u/" + weiboId,
                });
            }
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
                OriginalRSSType.Bilibili => await ProcessingBilibili(originalRSS, entries, users),
                OriginalRSSType.HeyBox => await ProcessingHeyBox(originalRSS, entries, users),
                _ => null
            };
        }

        #region 微博

        public async Task<GameNews> ProcessingMicroblog(OriginalRSS originalRSS, List<string> entries, List<WeiboUserInfor> users)
        {
            var authorString = GetMicroblogAuthor(originalRSS.Description);
            var model = new GameNews
            {
                Type = ArticleType.News,
                RSS = originalRSS,
                Title = (await GetMicroblogTitle(originalRSS.Title, originalRSS.Description, authorString, 15)).Trim().Replace("\n", "").Replace("\r", ""),
                BriefIntroduction = await GetMicroblogTitle(originalRSS.Title, originalRSS.Description, authorString, 500),
                Author = authorString,
                MainPage = await GetMicroblogMainPage(originalRSS.Description, authorString),
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

            var markdown = converter.Convert(description).Replace("\\[\\]", "[]");

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
                    markdown = markdown.Replace($"<video{videoHtml}</video>", $"![]({await _fileUploadService.TransformImageAsync(image)})\n视频无法显示，请前往[微博视频]({link})观看\n");
                }

            }

            return markdown;
        }

        public async Task<string> GetMicroblogMainImage(string description)
        {
            var links = description.GetImageLinks();
            return links.Any() ? await _fileUploadService.TransformImageAsync(links.Last(), 460, 215) : "";
        }

        #endregion

        #region B站

        public string GetBilibiliAuthor(string description)
        {
            return ToolHelper.MidStrEx(description, "//转发自: @", ":");
        }

        public string GetBilibiliTitle(string description, string author, int maxLength)
        {
            var title = description.Split($"//转发自: @{author}:").Last().Trim().Replace("<br>", "\n");
            // 去掉title开头的换行符
            if (title.StartsWith("\n"))
            {
                title = title.Substring(1);
            }

            title = title.Split("\n").First();

            return _appHelper.GetStringAbbreviation(title, maxLength);
        }

        public string GetBilibiliBriefIntroduction(string description, string author, int maxLength)
        {
            // 转成markdowm 方便去除图片
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            var title = Markdown.ToPlainText(GetBilibiliMainPage(description, author), pipeline);


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



            return _appHelper.GetStringAbbreviation(title.Trim(), maxLength);
        }

        public string GetBilibiliMainPage(string description, string author)
        {
            var text = description.Split($"//转发自: @{author}: ").Last();

            var title = text.Split("\n").First();

            // 如果标题小于50字，在正文里吧标题去掉
            string brief;
            if (title.Length > 50)
            {
                brief = text;
            }
            else
            {
                brief = string.Join("\n", text.Split("\n").Skip(1));
            }

            // 去除末尾的链接
            var link = GetBilibiliLink(description);
            brief = brief.Replace($"图文地址：<a href=\"{link}\">{link}</a>", "").Replace($"视频地址：<a href=\"{link}\">{link}</a>", "").Replace($"专栏地址：<a href=\"{link}\">{link}</a>", "");

            var converter = new ReverseMarkdown.Converter();

            var markdown = converter.Convert(brief).Replace("\\[\\]", "[]");

            //去除 视频
            do
            {
                var midStr = ToolHelper.MidStrEx(markdown, "<iframe ", "</iframe>");
                var bvid = ToolHelper.MidStrEx(midStr, "bvid=", "\"");

                markdown = markdown.Replace($"<iframe {midStr}</iframe>", $"\n[](https://www.bilibili.com/video/{bvid})\n");

            } while (string.IsNullOrWhiteSpace(ToolHelper.MidStrEx(markdown, "<iframe ", "</iframe>")) == false);


            return markdown;
        }

        public async Task<string> GetBilibiliMainImage(string description)
        {
            var links = description.GetImageLinks();
            return links.Any() ? await _fileUploadService.TransformImageAsync(links.Last(), 460, 215) : "";
        }

        public string GetBilibiliLink(string description)
        {
            var link = description.Split("地址：<a href=\"https://www.bilibili.com/").Last();
            return ToolHelper.MidStrEx(link, "\">", "</a>");
        }

        public async Task<GameNews> ProcessingBilibili(OriginalRSS originalRSS, List<string> entries, List<WeiboUserInfor> users)
        {
            var authorString = GetBilibiliAuthor(originalRSS.Description);
            var model = new GameNews
            {
                Type = ArticleType.News,
                RSS = originalRSS,
                Title = (GetBilibiliTitle(originalRSS.Description, authorString, 20)),
                BriefIntroduction = GetBilibiliBriefIntroduction(originalRSS.Description, authorString, 500),
                Author = authorString,
                MainPage = GetBilibiliMainPage(originalRSS.Description, authorString),
                Link = originalRSS.Link,
                MainPicture = await GetBilibiliMainImage(GetBilibiliMainPage(originalRSS.Description, authorString)),
                PublishTime = originalRSS.PublishTime,
                State = GameNewsState.Edit
            };

            //检查是否重复
            if (await _gameNewsRepository.GetAll().Include(s => s.RSS).AnyAsync(s => (s.RSS.Link == model.Link || s.Link == model.Link) && s.Title != "已删除"))
            {
                throw new Exception("该B站动态已存在");
            }

            //修正作者
            if (string.IsNullOrWhiteSpace(model.Author))
            {
                model.Author = originalRSS.Author;
            }

            //查找关联词条
            var relatedEntries = new List<string>();
            foreach (var item in entries)
            {
                if (originalRSS.Description.Contains(item))
                {
                    relatedEntries.Add(item);
                }
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

            return model;
        }
        #endregion

        #region 小黑盒

        public async Task<GameNews> ProcessingHeyBox(OriginalRSS originalRSS, List<string> entries, List<WeiboUserInfor> users)
        {
            var authorString = originalRSS.Author;
            var model = new GameNews
            {
                Type = ArticleType.News,
                RSS = originalRSS,
                Title = GetHeyBoxTitle(originalRSS.Title, 30),
                BriefIntroduction = GetHeyBoxBriefIntroduction(originalRSS.Description, 500),
                Author = authorString,
                MainPage = GetHeyBoxMainPage(originalRSS.Description),
                Link = originalRSS.Link,
                MainPicture = await GetHeyBoxMainImage(originalRSS.Description),
                PublishTime = originalRSS.PublishTime,
                State = GameNewsState.Edit,
                IsOriginal = true
            };

            //检查是否重复
            if (await _gameNewsRepository.GetAll().Include(s => s.RSS).AnyAsync(s => (s.RSS.Link == model.Link || s.Link == model.Link) && s.Title != "已删除"))
            {
                throw new Exception("该小黑盒动态已存在");
            }

            //查找关联词条
            var relatedEntries = new List<string>();
            foreach (var item in entries)
            {
                if (originalRSS.Title.Contains(item) || originalRSS.Description.Contains(item))
                {
                    relatedEntries.Add(item);
                }
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

            // 如果找到了关联词条且有主图，可以考虑自动发布
            //if (model.Entries.Count > 0 && !string.IsNullOrWhiteSpace(model.MainPicture))
            //{
            //    model.State = GameNewsState.Publish;
            //}

            return model;
        }

        public string GetHeyBoxTitle(string title, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return "未命名";
            }

            // 清理标题中的特殊字符和表情
            var cleanTitle = Regex.Replace(title, @"\[[\w_!]+\]", "");
            cleanTitle = cleanTitle.Trim();

            return _appHelper.GetStringAbbreviation(cleanTitle, maxLength);
        }

        public string GetHeyBoxBriefIntroduction(string description, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return "";
            }

            // 转换为纯文本
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            var plainText = Markdown.ToPlainText(GetHeyBoxMainPage(description), pipeline);

            // 清理表情符号
            plainText = Regex.Replace(plainText, @"\[[\w_!]+\]", "");

            //去除图片
            do
            {
                var midStr = ToolHelper.MidStrEx(plainText, "[", "]");
                plainText = plainText.Replace($"[{midStr}]", "");
            } while (string.IsNullOrWhiteSpace(ToolHelper.MidStrEx(plainText, "[", "]")) == false);

            return _appHelper.GetStringAbbreviation(plainText.Trim(), maxLength);
        }

        public string GetHeyBoxMainPage(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return "";
            }

            // 清理小黑盒特有的表情标记
            var text = description;

            // 将HTML转换为Markdown
            var converter = new ReverseMarkdown.Converter();
            var markdown = converter.Convert(text).Replace("\\[\\]", "[]");

            return markdown;
        }

        public async Task<string> GetHeyBoxMainImage(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return "";
            }

            var links = description.GetImageLinks();
            return links.Any() ? await _fileUploadService.TransformImageAsync(links.First(), 460, 215) : "";
        }

        #endregion

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
