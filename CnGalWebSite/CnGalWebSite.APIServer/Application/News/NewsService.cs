using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using CnGalWebSite.APIServer.ExamineX;
using Microsoft.AspNetCore.Identity;
using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Collections.Concurrent;
using ReverseMarkdown.Converters;
using Markdig;

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
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<GameNews>, string, SortOrder, IEnumerable<GameNews>>> SortLambdaCacheGameNews = new();
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<WeeklyNews>, string, SortOrder, IEnumerable<WeeklyNews>>> SortLambdaCacheWeeklyNews = new();


        public NewsService(IConfiguration configuration, IRSSHelper rssHelper, IAppHelper appHelper, IRepository<Entry, int> entryRepository,
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
        }


        public Task<QueryData<ListGameNewAloneModel>> GetPaginatedResult(QueryPageOptions options, ListGameNewAloneModel searchModel)
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

            // 处理 Searchable=true 列与 SeachText 模糊搜索
            if (options.Searchs.Any())
            {

                // items = items.Where(options.Searchs.GetFilterFunc<Entry>(FilterLogic.Or));
            }
            else
            {
                // 处理 SearchText 模糊搜索
                if (!string.IsNullOrWhiteSpace(options.SearchText))
                {
                    items = items.Where(item => (item.Author?.Contains(options.SearchText) ?? false)
                    || (item.Title?.Contains(options.SearchText) ?? false)
                    || (item.BriefIntroduction?.Contains(options.SearchText) ?? false));
                }
            }
            // 过滤
            /* var isFiltered = false;
             if (options.Filters.Any())
             {
                 items = items.Where(options.Filters.GetFilterFunc<Entry>());
                 isFiltered = true;
             }*/

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheGameNews.GetOrAdd(typeof(GameNews), key => LambdaExtensions.GetSortLambda<GameNews>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
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
                    ArticleId = item.ArticleId??0,
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

        public Task<QueryData<ListWeeklyNewAloneModel>> GetPaginatedResult(QueryPageOptions options, ListWeeklyNewAloneModel searchModel)
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

            // 处理 Searchable=true 列与 SeachText 模糊搜索
            if (options.Searchs.Any())
            {

                // items = items.Where(options.Searchs.GetFilterFunc<Entry>(FilterLogic.Or));
            }
            else
            {
                // 处理 SearchText 模糊搜索
                if (!string.IsNullOrWhiteSpace(options.SearchText))
                {
                    items = items.Where(item => (item.Title?.Contains(options.SearchText) ?? false)
                    || (item.BriefIntroduction?.Contains(options.SearchText) ?? false));
                }
            }
            // 过滤
            /* var isFiltered = false;
             if (options.Filters.Any())
             {
                 items = items.Where(options.Filters.GetFilterFunc<Entry>());
                 isFiltered = true;
             }*/

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheWeeklyNews.GetOrAdd(typeof(WeeklyNews), key => LambdaExtensions.GetSortLambda<WeeklyNews>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
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
                    ArticleId = item.ArticleId??0,
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


        public async Task UpdateNewestGameNews()
        {
            //查找最新的RSS源的时间

            var time = await _gameNewsRepository.GetAll().AnyAsync() ?
                await _gameNewsRepository.GetAll().Include(s => s.RSS).MaxAsync(s => s.RSS.PublishTime) : DateTime.MinValue;
            var weiboes = await _rssHelper.GetOriginalWeibo(long.Parse(_configuration["RSSWeiboUserId"]),time);

            if(weiboes.Count == 0)
            {
                return;
            }
            //获取周报
            var weekly=await _weeklyNewsRepository.GetAll().Include(s=>s.News).OrderByDescending(s => s.CreateTime).FirstOrDefaultAsync();
            if (weekly == null || weekly.CreateTime.IsInSameWeek( DateTime.Now.ToCstTime()) == false)
            {
                weekly = await GenerateNewestWeeklyNews();
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
  
        public async Task UpdateWeiboUserInforCache()
        {
            //获取所有Staff制作组的名称和微博Id
            //检查是否以及缓存
            //没有缓存则缓存

            var currentUserIds = await _weiboUserInforRepository.GetAll().AsNoTracking().Select(s => s.EntryId).ToListAsync();

            var staffs = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Information)
                .Where(s => (s.Type == EntryType.Staff || s.Type == EntryType.ProductionGroup) && currentUserIds.Contains(s.Id) == false &&
                  s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.Information.Any())
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Information
                }).ToListAsync();

            var staffWeiboes=new List<KeyValuePair<string,long>>();

            for(int i = 0;i< staffs.Count; i++)
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
                            staffWeiboes.Add(new KeyValuePair<string, long>( staff.Name,long.Parse(temp.Last())));
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
                    AddWeiboUserInfor(item.Key, item.Value);
                }
                catch (Exception ex)
                {

                }

            }
        }

        public async Task<WeeklyNews> GenerateNewestWeeklyNews()
        {
            //查找上一次周报时间

            var last = await _weeklyNewsRepository.GetAll().Include(s => s.News).OrderByDescending(s => s.CreateTime).FirstOrDefaultAsync();

            bool isNeedCreate = false;
            if (last == null)
            {
                isNeedCreate = true;
            }
            else
            {

                isNeedCreate = DateTime.Now.ToCstTime().IsInSameWeek(last.CreateTime);
            }

            if (isNeedCreate)
            {
                //发表旧周报
                if (last!=null&& last.State == GameNewsState.Edit)
                {
                    await PublishWeeklyNews(last);
                }

                //创建新周报
                last = await _weeklyNewsRepository.InsertAsync(new WeeklyNews
                {
                    CreateTime = DateTime.Now.ToCstTime(),
                    Type =ArticleType.News
                });

            }

            return last;
        }

        public async Task PublishNews(GameNews gameNews)
        {
            Article article = new Article
            {
                Name = gameNews.Title,
                OriginalAuthor = gameNews.Author,
                OriginalLink = gameNews.Link,
                MainPage = gameNews.MainPage,
                BriefIntroduction = gameNews.BriefIntroduction,
                MainPicture = gameNews.MainPicture,
                Type = gameNews.Type,
                NewsType = "动态",
                CreateUserId = _configuration["NewsAdminId"],
                PubishTime=gameNews.PublishTime,
                RealNewsTime=gameNews.PublishTime
            };

            //添加关联信息
            if (gameNews.Entries.Any())
            {
                var entryNames = gameNews.Entries.Select(s => s.EntryName);
                var entries = await _entryRepository.GetAll().Where(s => entryNames.Contains(s.Name)).Select(s => new
                {
                    s.Name,
                    s.Type
                }).ToListAsync();

                foreach (var entry in entries)
                {
                    article.Relevances.Add(new ArticleRelevance
                    {
                        Modifier = entry.Type.GetDisplayName(),
                        DisplayName = entry.Name,
                    });
                }
            }

            //检查是否合规
            if (string.IsNullOrWhiteSpace(article.Name))
            {
                throw new Exception("名称不能为空");
            }
            //不能重名
            if (await _articleRepository.CountAsync(s => s.Name == article.Name) > 0)
            {
                throw new Exception("文章名称名称『" + article.Name + "』重复，请尝试使用显示名称");

            }

            //走批量导入的流程
            var admin = await _userManager.FindByIdAsync(article.CreateUserId);
            await _examineService.AddBatchArticleExaminesAsync(article, admin, "自动生成动态");

            //查找文章
            var articleId = await _articleRepository.GetAll().Where(s => s.Name == article.Name).Select(s => s.Id).FirstOrDefaultAsync();

            gameNews.ArticleId= articleId;

            await _gameNewsRepository.UpdateAsync(gameNews);

            //添加到当天周报
            //获取周报
            var weekly = await _weeklyNewsRepository.GetAll().Include(s => s.News).OrderByDescending(s => s.CreateTime).FirstOrDefaultAsync();
            if (weekly == null || weekly.CreateTime.IsInSameWeek(DateTime.Now.ToCstTime()) == false)
            {
                weekly = await GenerateNewestWeeklyNews();
            }

            if(weekly.News.Any(s=>s.Id==gameNews.Id)==false)
            {
                weekly.News.Add(gameNews);
                await _weeklyNewsRepository.UpdateAsync(weekly);
            }
        }

        public async Task PublishWeeklyNews(WeeklyNews weeklyNews)
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
            var mainPage=GenerateRealWeeklyNewsMainPage(weeklyNews);

            Article article = new Article
            {
                Name = weeklyNews.Title,
                MainPage = mainPage,
                BriefIntroduction = weeklyNews.BriefIntroduction,
                MainPicture = weeklyNews.MainPicture,
                Type =ArticleType.News,
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
                throw new Exception("文章名称名称『" + article.Name + "』重复，请尝试使用显示名称");

            }

            //走批量导入的流程
            var admin = await _userManager.FindByIdAsync(article.CreateUserId);
            await _examineService.AddBatchArticleExaminesAsync(article, admin, "自动生成动态");

            //查找文章
            var articleId = await _articleRepository.GetAll().Where(s => s.Name == article.Name).Select(s => s.Id).FirstOrDefaultAsync();

            weeklyNews.ArticleId = articleId;
            weeklyNews.PublishTime = DateTime.Now.ToCstTime();

            await _weeklyNewsRepository.UpdateAsync(weeklyNews);
        }

        public WeeklyNews ResetWeeklyNews(WeeklyNews weeklyNews)
        {
            weeklyNews.Title = GenerateWeeklyNewsTitle(weeklyNews);
            weeklyNews.BriefIntroduction = GenerateWeeklyNewsBriefIntroduction(weeklyNews);
            weeklyNews.MainPage = "";
            weeklyNews.MainPicture = GenerateWeeklyNewsMainImage(weeklyNews);

            return weeklyNews;
        }

        public async Task AddWeiboUserInfor(string entryName,long weiboId)
        {
            var user=await _rssHelper.GetWeiboUserInfor(weiboId);

            var entry = await _entryRepository.GetAll().Include(s=>s.Information).FirstOrDefaultAsync(s => s.Name == entryName);

            //添加主图
            entry.MainPicture = entry.Thumbnail = user.Image;
            entry.Information.Remove(entry.Information.FirstOrDefault(s => s.DisplayName == "微博"));
            entry.Information.Add(new BasicEntryInformation
            {
                Modifier = "相关网站",
                DisplayName = "微博",
                DisplayValue = "https://weibo.com/u/" + weiboId,
            });

            user.EntryId = entry.Id;

            await _weiboUserInforRepository.InsertAsync(user);

            await _entryRepository.UpdateAsync(entry);
        }

        public  string GenerateWeeklyNewsTitle(WeeklyNews weeklyNews)
        {
            return "CnGal周报（" + weeklyNews.CreateTime.AddDays(-(int)weeklyNews.CreateTime.DayOfWeek).ToString("yyyy.M.d") + " - " + weeklyNews.CreateTime.AddDays(7 - (int)weeklyNews.CreateTime.DayOfWeek).ToString("yyyy.M.d") + "）";
        }

        public string GenerateWeeklyNewsBriefIntroduction(WeeklyNews weeklyNews)
        {
            string model = "";
            foreach(var item in weeklyNews.News)
            {
                model += "【" + item.Author + "】" + item.Title;
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
            var strList = weeklyNews.MainPage.Split("[Foot]");
            var model = strList[0] + "\n";

            //目录
            model += "## 目录\n";
            foreach (var item in weeklyNews.News)
            {
                model += "- " + item.Author + "" + item.Title + "\n";
            }
            //正文
            foreach (var item in weeklyNews.News)
            {
                model += "## " + item.Author + " - " + item.Title + "\n" + item.MainPage + "\n" + "[原文链接](" + item.Link + ")\n" + "![image](" + item.MainPicture + ")\n";
            }

            if(strList.Count()>1)
            {
                model += strList[1];
            }

            return model;
        }


        public async Task<GameNews> ProcessingOriginalRSS(OriginalRSS originalRSS)
        {
            var entries = await _entryRepository.GetAll().Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Name).ToListAsync();
            var users = await _weiboUserInforRepository.GetAll().Include(s => s.Entry).ToListAsync();

            return originalRSS.Type switch
            {
                OriginalRSSType.Weibo =>  ProcessingMicroblog(originalRSS,entries,users),
                _ => null
            };
        }

        public GameNews ProcessingMicroblog(OriginalRSS originalRSS,List<string> entries,List<WeiboUserInfor> users)
        {
            var authorString = GetMicroblogAuthor(originalRSS.Title);
            GameNews model = new GameNews
            {
                Type = ArticleType.News,
                RSS = originalRSS,
                Title = GetMicroblogTitle(originalRSS.Title,originalRSS.Description, authorString, 15),
                BriefIntroduction = GetMicroblogTitle(originalRSS.Title, originalRSS.Description, authorString, 50),
                Author = GetMicroblogAuthor(originalRSS.Description),
                MainPage = GetMicroblogMainPage(originalRSS.Description, authorString),
                Link=originalRSS.Link,
                MainPicture=GetMicroblogMainImage(originalRSS.Description),
                PublishTime=originalRSS.PublishTime,
                State=GameNewsState.Edit
            };

            //查找关联词条
            var relatedEntries =new List<string>();
            foreach(var item in entries)
            {
                if(model.MainPage.Contains(item))
                {
                    relatedEntries.Add(item);
                }
            }

            //查找作者
            var author= users.FirstOrDefault(s => s.WeiboName == model.Author);
            bool isAuthor = author != null;
            if(author!=null)
            {
                relatedEntries.Add(author.Entry.Name);
            }

            //过滤关联词条
            relatedEntries = ScreenRelatedEntry(relatedEntries);

            foreach(var item in relatedEntries)
            {
                model.Entries.Add(new GameNewsRelatedEntry
                {
                    EntryName = item
                });
            }

            //如果找到了关联词条 也找到了 作者 而且有主图 那么直接发布
            if (isAuthor && model.Entries.Count > 1 && string.IsNullOrWhiteSpace(model.MainPicture) == false)
            {
                model.State = GameNewsState.Publish;
            }

            return model;
        }

        public List<string> ScreenRelatedEntry(List<string> entries)
        {
            List<string> result = new List<string>();

            foreach(var entry in entries)
            {
                long temp = 0;
                if(long.TryParse(entry,out temp)==false)
                {
                    if(entry!="Steam")
                    {
                        result.Add(entry);
                    }
                }
            }

            return result;
        }

        public string GetMicroblogTitle(string title, string description, string author, int maxLength)
        {
            //分割出转发之前的文本
            var temp = title.Split(":&ensp;");
            if (temp.Length <= 1)
            {
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
                title = Markdown.ToPlainText(GetMicroblogMainPage(description, author), pipeline);
                if(title.Split(": ").Length>1)
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
            if(temp.Length>1)
            {
                description = temp[1];
            }
            return ToolHelper.MidStrEx(description, "@", "</a>");
        }

  /*      public string GetMicroblogBriefIntroduction(string description)
        {
            var str=StripHTML(description);
            return _appHelper.GetStringAbbreviation(GetMicroblogTitle(str), 50);
        }*/

        public string GetMicroblogMainPage(string description,string author)
        {
            var temp = description.Split("- 转发");
            if (temp.Length > 1)
            {
                description = temp[1];
            }
            temp = description.Split("@"+author+ "</a>: ");
            if (temp.Length > 1)
            {
                description = temp[1];
            }
        

            var converter = new ReverseMarkdown.Converter();

            return converter.Convert(description);
        }

        public string GetMicroblogMainImage(string description)
        {
            var links = ToolHelper.GetImageLinks(description);
            return links.Any() ? links[0] : "";
        }


        /// <summary>
        /// 去除HTML标记
        /// </summary>
        /// <param name="strHtml">包括HTML的源码 </param>
        /// <returns>已经去除后的文字</returns>
        public static string StripHTML(string strHtml)
        {
            string[] aryReg ={
                @"<script[^>]*?>.*?</script>",

                @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
                @"([\r\n])[\s]+",
                @"&(quot|#34);",
                @"&(amp|#38);",
                @"&(lt|#60);",
                @"&(gt|#62);",
                @"&(nbsp|#160);",
                @"&(iexcl|#161);",
                @"&(cent|#162);",
                @"&(pound|#163);",
                @"&(copy|#169);",
                @"&#(\d+);",
                @"-->",
                @"<!--.*\n"

            };

            string[] aryRep = {
                "",
                "",
                "",
                "\"",
                "&",
                "<",
                ">",
                " ",
                "\xa1",//chr(161),
                "\xa2",//chr(162),
                "\xa3",//chr(163),
                "\xa9",//chr(169),
                "",
                "\r\n",
                ""
            };

            string newReg = aryReg[0];
            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, aryRep[i]);
            }

            strOutput.Replace("<", "");
            strOutput.Replace(">", "");
            strOutput.Replace("\r\n", "");


            return strOutput;
        }
    }
}
