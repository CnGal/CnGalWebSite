using Markdig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Search;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/articles/[action]")]
    public class ArticlesAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<ThumbsUp, long> _thumbsUpRepository;
        private readonly IRepository<Comment, long> _commentUpRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IArticleService _articleService;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;


        public ArticlesAPIController(IArticleService articleService, IRepository<Comment, long> commentUpRepository, IRepository<ThumbsUp, long> thumbsUpRepository,
            IExamineService examineService,
        UserManager<ApplicationUser> userManager, IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Examine, long> examineRepository,
        IRepository<Entry, int> entryRepository)
        {
            _userManager = userManager;
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _thumbsUpRepository = thumbsUpRepository;
            _commentUpRepository = commentUpRepository;
            _articleService = articleService;
            _examineRepository = examineRepository;
            _examineService = examineService;
        }

        /// <summary>
        /// 通过Id获取文章的真实数据 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Article>> GetArticleDataAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var article = await _articleRepository.GetAll().AsNoTracking().Include(s => s.Relevances).Include(s => s.Examines).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id);

            if (article == null)
            {
                return NotFound();
            }
            else
            {
                //判断当前是否隐藏
                if (article.IsHidden == true)
                {
                    if (user == null || await _userManager.IsInRoleAsync(user, "Admin") != true)
                    {
                        return NotFound();
                    }
                    article.IsHidden = true;
                }
                else
                {
                    article.IsHidden = false;

                }
                //需要清除环回引用
                foreach (var item in article.Examines)
                {
                    item.Article = null;
                }
                return article;
            }

        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleViewModel>> GetArticleViewAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取文章
            var article = await _articleRepository.GetAll().AsNoTracking().Include(s => s.Disambig)
                .Include(s => s.CreateUser).Include(s => s.ThumbsUps).Include(s => s.Relevances).Include(s => s.Examines).FirstOrDefaultAsync(x => x.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            List<Examine> examineQuery = null;
            //读取审核信息
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll()
                               .Where(s => s.ArticleId == article.Id && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleMainPage || s.Operation == Operation.EditArticleRelevanes))
                               .Select(s => new Examine
                               {
                                   Operation = s.Operation,
                                   Context = s.Context
                               })
                               .ToListAsync();
            }

            //读取当前登入用户审核信息 获取待审核的内容
            Examine examine = null;
            if (user != null)
            {
                examine = examineQuery.Find(s => s.Operation == Operation.EditArticleMain);
                if (examine != null)
                {
                    _articleService.UpdateArticleData(article, examine);
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EditArticleMainPage);

                if (examine != null)
                {
                    _articleService.UpdateArticleData(article, examine);
                }
            }
            //建立视图模型
            var model = new ArticleViewModel
            {
                Id = article.Id,
                Name = article.DisplayName ?? article.Name,
                Type = article.Type,
                MainPage = article.MainPage,
                PubishTime = article.RealNewsTime ?? article.PubishTime,
                OriginalLink = article.OriginalLink,
                OriginalAuthor = article.OriginalAuthor,
                CreateTime = article.CreateTime,
                ThumbsUpCount = article.ThumbsUps.Count,
                ReaderCount = article.ReaderCount,
                EditDate = article.LastEditTime,
                CommentCount = await _commentUpRepository.LongCountAsync(s => s.ArticleId == id),
                CanComment = article.CanComment ?? true,
                DisambigId = article.DisambigId ?? 0,
                DisambigName = article.Disambig?.Name,
                BriefIntroduction = article.BriefIntroduction
            };

            if (user != null)
            {
                examine = examineQuery.Find(s => s.Operation == Operation.EditArticleMain);
                if (examine != null)
                {
                    model.MainState = EditState.Preview;
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EditArticleMainPage);

                if (examine != null)
                {
                    model.MainPageState = EditState.Preview;
                }
            }


            //判断当前是否隐藏
            if (article.IsHidden == true)
            {
                if (user == null || await _userManager.IsInRoleAsync(user, "Admin") != true)
                {
                    return NotFound();
                }
                model.IsHidden = true;
            }
            else
            {
                model.IsHidden = false;
            }



            //初始化图片
            model.MainPicture = _appHelper.GetImagePath(article.MainPicture, "");
            model.BackgroundPicture = _appHelper.GetImagePath(article.BackgroundPicture, "");
            model.SmallBackgroundPicture = _appHelper.GetImagePath(article.SmallBackgroundPicture, "");

            //初始化主页Html代码
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            model.MainPage = Markdown.ToHtml(model.MainPage ?? "", pipeline);

            //复制数据
            var createUser = article.CreateUser;
            if (createUser == null)
            {
                createUser = article.Examines.First(s => s.IsPassed == true).ApplicationUser;
            }


            model.LastEditUserName = createUser.UserName;
            model.UserId = createUser.Id;

            //判断是否有权限编辑
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin") == true)
            {
                model.Authority = true;
            }
            else
            {
                if (user != null && user.Id == article.CreateUser.Id)
                {
                    model.Authority = true;
                }
                else
                {
                    model.Authority = false;
                }
            }

            //判断是否已经点赞
            if (user != null && article.ThumbsUps != null)
            {

                if (article.ThumbsUps.Find(s => s.ApplicationUserId == user.Id) == null)
                {
                    model.IsThumbsUp = false;
                }
                else
                {
                    model.IsThumbsUp = true;
                }
            }
            else
            {
                model.IsThumbsUp = false;
            }



            //序列化相关性列表
            //读取当前用户等待审核的信息
            if (user != null)
            {
                examine = examineQuery.Find(s => s.Operation == Operation.EditArticleRelevanes);
                if (examine != null)
                {
                    model.RelevancesState = EditState.Preview;
                    _articleService.UpdateArticleData(article, examine);
                }
            }
            //读取词条信息
            var relevances = new List<RelevancesViewModel>();
            foreach (var item in article.Relevances)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {
                    if (infor.Modifier == item.Modifier)
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new RelevancesKeyValueModel
                        {
                            DisplayName = item.DisplayName,
                            DisplayValue = item.DisplayValue,
                            Link = item.Link
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new RelevancesViewModel
                    {
                        Modifier = item.Modifier,
                        Informations = new List<RelevancesKeyValueModel>()
                    };
                    temp.Informations.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue,
                        Link = item.Link

                    });
                    relevances.Add(temp);
                }
            }


            //加载附加信息 关联词条获取
            foreach (var item in relevances)
            {
                switch (item.Modifier)
                {
                    case "角色":
                        foreach (var infor in item.Informations)
                        {
                            //获取角色词条
                            var temp = await _entryRepository.FirstOrDefaultAsync(x => x.Name == infor.DisplayName && x.Type == EntryType.Role);
                            if (temp != null)
                            {
                                infor.DisplayName = temp.DisplayName ?? infor.DisplayName;
                                infor.Image = _appHelper.GetImagePath(temp.Thumbnail, "user.png");
                                infor.Link = "/entries/index/" + temp.Id;
                            }
                            else
                            {
                                infor.Image = _appHelper.GetImagePath(null, "user.png");
                            }
                        }
                        break;
                    case "STAFF":
                        foreach (var infor in item.Informations)
                        {
                            //获取STAFF词条
                            var temp = await _entryRepository.FirstOrDefaultAsync(x => x.Name == infor.DisplayName && x.Type == EntryType.Staff);
                            if (temp != null)
                            {
                                infor.DisplayName = temp.DisplayName ?? infor.DisplayName;
                                infor.Image = _appHelper.GetImagePath(temp.Thumbnail, "user.png");
                                infor.Link = "/entries/index/" + temp.Id;
                            }
                            else
                            {
                                infor.Image = _appHelper.GetImagePath(null, "user.png");
                            }
                        }
                        break;
                    case "游戏":
                        foreach (var infor in item.Informations)
                        {
                            //获取STAFF词条
                            var temp = await _entryRepository.FirstOrDefaultAsync(x => x.Name == infor.DisplayName && x.Type == EntryType.Game);
                            if (temp != null)
                            {
                                infor.DisplayName = temp.DisplayName ?? infor.DisplayName;
                                infor.DisplayValue = _appHelper.GetStringAbbreviation(temp.BriefIntroduction, 20);
                                infor.Image = _appHelper.GetImagePath(temp.MainPicture, "app.png");
                                infor.Link = "/entries/index/" + temp.Id;
                            }
                            else
                            {
                                infor.Image = _appHelper.GetImagePath(null, "app.png");
                            }
                        }
                        break;
                    case "制作组":
                        foreach (var infor in item.Informations)
                        {
                            //获取STAFF词条
                            var temp = await _entryRepository.FirstOrDefaultAsync(x => x.Name == infor.DisplayName && x.Type == EntryType.ProductionGroup);
                            if (temp != null)
                            {
                                infor.DisplayName = temp.DisplayName ?? infor.DisplayName;
                                infor.DisplayValue = _appHelper.GetStringAbbreviation(temp.BriefIntroduction, 20);
                                infor.Image = _appHelper.GetImagePath(temp.MainPicture, "app.png");
                                infor.Link = "/entries/index/" + temp.Id;
                            }
                            else
                            {
                                infor.Image = _appHelper.GetImagePath(null, "app.png");
                            }
                        }
                        break;
                    case "文章":
                        foreach (var infor in item.Informations)
                        {
                            //获取STAFF词条
                            var temp = await _articleRepository.FirstOrDefaultAsync(x => x.Name == infor.DisplayName);
                            if (temp != null)
                            {
                                infor.DisplayName = temp.DisplayName ?? infor.DisplayName;
                                infor.DisplayValue = _appHelper.GetStringAbbreviation(temp.BriefIntroduction, 20);
                                infor.Image = _appHelper.GetImagePath(temp.MainPicture, "certificate.png");
                                infor.Link = "/articles/index/" + temp.Id;
                            }
                            else
                            {
                                infor.Image = _appHelper.GetImagePath(null, "certificate.png");
                            }
                        }
                        break;
                }
            }

            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.ArticleId == article.Id && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }

            //获取各部分状态
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditArticleMain))
                    {
                        model.MainState = EditState.locked;
                    }
                    else
                    {
                        model.MainState = EditState.Normal;
                    }
                }
                if (model.MainPageState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditArticleMainPage))
                    {
                        model.MainPageState = EditState.locked;
                    }
                    else
                    {
                        model.MainPageState = EditState.Normal;
                    }
                }
                if (model.RelevancesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditArticleRelevanes))
                    {
                        model.RelevancesState = EditState.locked;
                    }
                    else
                    {
                        model.RelevancesState = EditState.Normal;
                    }
                }
            }

            //赋值
            model.Relevances = relevances;


            //增加文章阅读次数
            await _appHelper.ArticleReaderNumUpAsync(article.Id);

            return model;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleAsyncInforViewModel>> GetArticleAsyncInfor(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取文章
            var article = await _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Include(s => s.ThumbsUps).FirstOrDefaultAsync(x => x.Id == id);
            if (article == null)
            {
                return NotFound();
            }
            //建立视图模型
            var model = new ArticleAsyncInforViewModel();
            //判断当前是否隐藏
            if (article.IsHidden == true)
            {
                if (user == null || await _userManager.IsInRoleAsync(user, "Admin") != true)
                {
                    return NotFound();
                }
            }
            //判断是否有权限编辑
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin") == true)
            {
                model.Authority = true;
            }
            else
            {
                if (user != null && user.Id == article.CreateUser.Id)
                {
                    model.Authority = true;
                }
                else
                {
                    model.Authority = false;
                }
            }
            //判断是否已经点赞
            if (article.ThumbsUps != null && article.ThumbsUps.Find(s => s.ApplicationUserId == user.Id) == null)
            {
                model.IsThumbsUp = false;
            }
            else
            {
                model.IsThumbsUp = true;
            }

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> CreateArticleAsync(CreateArticleViewModel model)
        {
            try
            {
                //获取当前用户ID
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                //检查是否超过编辑上限
                if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
                {
                    return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
                }
                //判断是否是上传图片

                //判断名称是否重复
                if (await _articleRepository.FirstOrDefaultAsync(s => s.Name == model.Name) != null)
                {
                    return new Result { Error = "该文章的名称与其他文章重复", Successful = false };
                }

                if (model.Type == ArticleType.Notice && await _userManager.IsInRoleAsync(user, "Admin") == false)
                {
                    return new Result { Error = "只有管理员才有权限发布公告", Successful = false };
                }

                //第一步 处理主要信息

                //新建审核数据对象
                var entryMain = new ArticleMain
                {
                    Name = model.Name,
                    BriefIntroduction = model.BriefIntroduction,
                    MainPicture = model.MainPicture,
                    BackgroundPicture = model.BackgroundPicture,
                    SmallBackgroundPicture = model.SmallBackgroundPicture,
                    Type = model.Type,
                    OriginalAuthor = model.OriginalAuthor,
                    OriginalLink = model.OriginalLink,
                    PubishTime = model.PubishTime,
                    RealNewsTime = model.RealNewsTime,
                    DisplayName = model.DisplayName,
                    NewsType = model.NewsType,
                };
                //序列化
                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, entryMain);
                    resulte = text.ToString();
                }
                //将空文章添加到数据库中 目的是为了获取索引
                var article = new Article
                {
                    Type = model.Type,
                    CreateUser = user,
                    CreateTime = DateTime.Now.ToCstTime(),
                    LastEditTime = DateTime.Now.ToCstTime()
                };
                article = await _articleRepository.InsertAsync(article);
                //判断是否是管理员
                if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                {
                    await _examineService.ExamineEditArticleMainAsync(article, entryMain);
                    await _examineService.UniversalCreateArticleExaminedAsync(article, user, true, resulte, Operation.EditArticleMain, model.Note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, article.Id, Operation.EditArticleMain);
                }
                else
                {
                    await _examineService.UniversalCreateArticleExaminedAsync(article, user, false, resulte, Operation.EditArticleMain, model.Note);
                }

                //第二步 处理关联词条

                //转化为标准词条相关性列表格式
                var entryRelevances = new List<EntryRelevance>();
                if (model.Roles != null)
                {
                    foreach (var item in model.Roles)
                    {
                        if (string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            entryRelevances.Add(new EntryRelevance
                            {
                                Modifier = "角色",
                                DisplayName = item.DisplayName
                            });
                        }

                    }
                }
                if (model.staffs != null)
                {
                    foreach (var item in model.staffs)
                    {
                        if (string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            entryRelevances.Add(new EntryRelevance
                            {
                                Modifier = "STAFF",
                                DisplayName = item.DisplayName
                            });
                        }
                    }
                }
                if (model.articles != null)
                {
                    foreach (var item in model.articles)
                    {
                        if (string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            entryRelevances.Add(new EntryRelevance
                            {
                                Modifier = "文章",
                                DisplayName = item.DisplayName
                            });
                        }
                    }
                }
                if (model.Groups != null)
                {
                    foreach (var item in model.Groups)
                    {
                        if (string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            entryRelevances.Add(new EntryRelevance
                            {
                                Modifier = "制作组",
                                DisplayName = item.DisplayName
                            });
                        }
                    }
                }
                if (model.Games != null)
                {
                    foreach (var item in model.Games)
                    {
                        if (string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            entryRelevances.Add(new EntryRelevance
                            {
                                Modifier = "游戏",
                                DisplayName = item.DisplayName
                            });
                        }
                    }
                }
                if (model.others != null)
                {
                    foreach (var item in model.others)
                    {
                        if (string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            entryRelevances.Add(new EntryRelevance
                            {
                                Modifier = "其他",
                                DisplayName = item.DisplayName,
                                DisplayValue = item.DisPlayValue,
                                Link = item.Link
                            });
                        }
                    }
                }
                //判断审核是否为空
                if (entryRelevances.Count != 0)
                {
                    //创建审核数据模型
                    var examinedModel = new ArticleRelecancesModel
                    {
                        Relevances = new List<ArticleRelevancesExaminedModel>()
                    };
                    foreach (var item in entryRelevances)
                    {
                        examinedModel.Relevances.Add(new ArticleRelevancesExaminedModel
                        {
                            IsDelete = false,
                            DisplayName = item.DisplayName,
                            Modifier = item.Modifier,
                            DisplayValue = item.DisplayValue,
                            Link = item.Link
                        });
                    }
                    //序列化JSON
                    resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, examinedModel);
                        resulte = text.ToString();
                    }
                    article = await _articleRepository.GetAll().Include(s => s.Relevances).FirstOrDefaultAsync(s => s.Id == article.Id);
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                    {
                        await _examineService.ExamineEditArticleRelevancesAsync(article, examinedModel);
                        await _examineService.UniversalCreateArticleExaminedAsync(article, user, true, resulte, Operation.EditArticleRelevanes, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, article.Id, Operation.EditArticleRelevanes);

                    }
                    else
                    {
                        await _examineService.UniversalCreateArticleExaminedAsync(article, user, false, resulte, Operation.EditArticleRelevanes, model.Note);
                    }
                }

                //第三步 添加正文

                //判断是否为空
                if (model.Context != null && string.IsNullOrWhiteSpace(model.Context) == false)
                {
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                    {
                        await _examineService.ExamineEditArticleMainPageAsync(article, model.Context);
                        await _examineService.UniversalCreateArticleExaminedAsync(article, user, true, model.Context, Operation.EditArticleMainPage, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, article.Id, Operation.EditArticleMainPage);

                    }
                    else
                    {
                        await _examineService.UniversalCreateArticleExaminedAsync(article, user, false, model.Context, Operation.EditArticleMainPage, model.Note);
                    }
                }

                return new Result { Successful = true, Error = article.Id.ToString() };
            }
            catch
            {
                return new Result { Error = "发表文章的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditArticleViewModel>> EditArticleAsync(long Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取词条
            var article = await _articleRepository.GetAll().AsNoTracking().Include(s => s.Relevances).FirstOrDefaultAsync(s => s.Id == Id);
            if (article == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && s.ArticleId == Id
            && (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleMainPage || s.Operation == Operation.EditArticleRelevanes)))
            {
                return NotFound();
            }

            var model = new EditArticleViewModel();
            //获取审核记录
            var examines = await _examineRepository.GetAllListAsync(s => s.ArticleId == article.Id && s.ApplicationUserId == user.Id
              && (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleMainPage || s.Operation == Operation.EditArticleRelevanes) && s.IsPassed == null);

            //第一步 获取主要信息
            var examine = examines.FirstOrDefault(s => s.Operation == Operation.EditArticleMain);
            if (examine != null)
            {
                _articleService.UpdateArticleData(article, examine);
            }
            model.MainPicturePath = _appHelper.GetImagePath(article.MainPicture, "app.png");
            model.BackgroundPicturePath = _appHelper.GetImagePath(article.BackgroundPicture, "app.png");
            model.MainPicture = article.MainPicture;
            model.BackgroundPicture = article.BackgroundPicture;

            model.Name = article.Name;
            model.BriefIntroduction = article.BriefIntroduction;
            model.Type = article.Type;
            model.OriginalAuthor = article.OriginalAuthor;
            model.OriginalLink = article.OriginalLink;
            model.PubishTime = article.PubishTime;

            model.NewsType = article.NewsType;
            model.RealNewsTime = article.RealNewsTime;
            model.DisplayName = article.DisplayName;

            model.SmallBackgroundPicture = article.SmallBackgroundPicture;
            model.SmallBackgroundPicturePath = _appHelper.GetImagePath(article.SmallBackgroundPicture, "app.png");
            model.Id = Id;


            //第二步 获取关联词条
            examine = examines.FirstOrDefault(s => s.Operation == Operation.EditArticleRelevanes);
            if (examine != null)
            {
                _articleService.UpdateArticleData(article, examine);
            }


            var roles = new List<RelevancesModel>();
            var staffs = new List<RelevancesModel>();
            var articles = new List<RelevancesModel>();
            var groups = new List<RelevancesModel>();
            var games = new List<RelevancesModel>();
            var others = new List<RelevancesModel>();
            foreach (var item in article.Relevances)
            {
                switch (item.Modifier)
                {
                    case "角色":
                        roles.Add(new RelevancesModel
                        {
                            DisplayName = item.DisplayName
                        });
                        break;
                    case "STAFF":
                        staffs.Add(new RelevancesModel
                        {
                            DisplayName = item.DisplayName
                        });
                        break;
                    case "文章":
                        articles.Add(new RelevancesModel
                        {
                            DisplayName = item.DisplayName
                        });
                        break;
                    case "制作组":
                        groups.Add(new RelevancesModel
                        {
                            DisplayName = item.DisplayName
                        });
                        break;
                    case "游戏":
                        games.Add(new RelevancesModel
                        {
                            DisplayName = item.DisplayName
                        });
                        break;
                    case "其他":
                        others.Add(new RelevancesModel
                        {
                            DisplayName = item.DisplayName,
                            DisPlayValue = item.DisplayValue,
                            Link = item.Link
                        });
                        break;
                }
            }
            model.Roles = roles;
            model.staffs = staffs;
            model.articles = articles;
            model.Groups = groups;
            model.Games = games;
            model.others = others;

            //第三步 获取正文
            examine = examines.FirstOrDefault(s => s.Operation == Operation.EditArticleMainPage);
            if (examine != null)
            {
                _articleService.UpdateArticleData(article, examine);
            }


            model.Context = article.MainPage;

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditArticleAsync(EditArticleViewModel model)
        {
            try
            {
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                //检查是否超过编辑上限
                if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
                {
                    return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
                }
                if (await _appHelper.CanUserEditArticleAsync(user, model.Id) == false)
                {
                    return new Result { Error = "权限不足，文章有其发布者和管理员才能编辑", Successful = false };
                }
                //判断是否为锁定状态
                if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId != user.Id && s.ArticleId == model.Id && s.IsPassed == null && (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleMainPage || s.Operation == Operation.EditArticleRelevanes)))
                {
                    return new Result { Error = "当前文章已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
                }
                if (model.Type == ArticleType.Notice && await _userManager.IsInRoleAsync(user, "Admin") == false)
                {
                    return new Result { Error = "只有管理员才有权限发布公告", Successful = false };
                }

                //查找当前文章
                var article = await _articleRepository.GetAll().Include(s => s.Relevances).FirstOrDefaultAsync(s => s.Id == model.Id);
                if (article == null)
                {
                    return new Result { Error = $"无法找到ID为{model.Id}的文章", Successful = false };
                }
                //判断名称是否重复
                if (model.Name != article.Name && await _articleRepository.FirstOrDefaultAsync(s => s.Name == model.Name && s.Id != model.Id) != null)
                {
                    return new Result { Error = "该文章的名称与其他文章重复", Successful = false };
                }

                //第一步 修改文章主要信息

                //判断是否修改
                if (article.SmallBackgroundPicture != model.SmallBackgroundPicture || article.DisplayName != model.DisplayName || article.NewsType != model.NewsType ||
                    article.BriefIntroduction != model.BriefIntroduction || article.MainPicture != model.MainPicture
                    || article.BackgroundPicture != model.BackgroundPicture || article.RealNewsTime != model.RealNewsTime || article.RealNewsTime != model.RealNewsTime
                    || article.Type != model.Type || article.OriginalLink != model.OriginalLink || article.OriginalAuthor != model.OriginalAuthor || article.PubishTime.ToString("D") != model.PubishTime.ToString("D"))
                {
                    //添加修改记录
                    //新建审核数据对象
                    var articleMain = new ArticleMain
                    {
                        Name = model.Name,
                        BriefIntroduction = model.BriefIntroduction,
                        MainPicture = model.MainPicture,
                        BackgroundPicture = model.BackgroundPicture,
                        Type = model.Type,
                        OriginalAuthor = model.OriginalAuthor,
                        OriginalLink = model.OriginalLink,
                        PubishTime = model.PubishTime,
                        RealNewsTime = model.RealNewsTime,
                        DisplayName = model.DisplayName,
                        NewsType = model.NewsType,
                        SmallBackgroundPicture = model.SmallBackgroundPicture
                    };
                    //序列化
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, articleMain);
                        resulte = text.ToString();
                    }
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                    {
                        await _examineService.ExamineEditArticleMainAsync(article, articleMain);
                        await _examineService.UniversalEditArticleExaminedAsync(article, user, true, resulte, Operation.EditArticleMain, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, article.Id, Operation.EditArticleMain);

                    }
                    else
                    {
                        await _examineService.UniversalEditArticleExaminedAsync(article, user, false, resulte, Operation.EditArticleMain, model.Note);
                    }
                }


                //第二步 修改文章关联词条

                //创建审核数据模型
                var examinedModel = new ArticleRelecancesModel
                {
                    Relevances = new List<ArticleRelevancesExaminedModel>()
                };
                //遍历当前词条数据 打上删除标签
                foreach (var item in article.Relevances)
                {
                    examinedModel.Relevances.Add(new ArticleRelevancesExaminedModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue,
                        IsDelete = true,
                        Link = item.Link
                    });
                }

                //再遍历视图 对应修改
                if (model.Roles != null)
                {
                    //循环查找是否相同
                    foreach (var item in model.Roles)
                    {
                        var isSame = false;
                        foreach (var infor in article.Relevances.Where(s => s.Modifier == "角色"))
                        {
                            if (item.DisplayName == infor.DisplayName)
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in examinedModel.Relevances)
                                {
                                    if (temp.DisplayName == infor.DisplayName)
                                    {
                                        examinedModel.Relevances.Remove(temp);
                                        isSame = true;
                                        break;
                                    }
                                }
                                isSame = true;
                                break;
                            }
                        }
                        if (isSame == false && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            examinedModel.Relevances.Add(new ArticleRelevancesExaminedModel
                            {
                                DisplayName = item.DisplayName,
                                Modifier = "角色",
                                IsDelete = false
                            });
                        }
                    }
                }
                if (model.staffs != null)
                {
                    //循环查找是否相同
                    foreach (var item in model.staffs)
                    {
                        var isSame = false;
                        foreach (var infor in article.Relevances.Where(s => s.Modifier == "STAFF"))
                        {
                            if (item.DisplayName == infor.DisplayName)
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in examinedModel.Relevances)
                                {
                                    if (temp.DisplayName == infor.DisplayName)
                                    {
                                        examinedModel.Relevances.Remove(temp);
                                        isSame = true;
                                        break;
                                    }
                                }
                                isSame = true;
                                break;
                            }
                        }
                        if (isSame == false && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            examinedModel.Relevances.Add(new ArticleRelevancesExaminedModel
                            {
                                DisplayName = item.DisplayName,
                                Modifier = "STAFF",
                                IsDelete = false
                            });
                        }
                    }
                }
                if (model.articles != null)
                {
                    //循环查找是否相同
                    foreach (var item in model.articles)
                    {
                        var isSame = false;
                        foreach (var infor in article.Relevances.Where(s => s.Modifier == "文章"))
                        {
                            if (item.DisplayName == infor.DisplayName)
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in examinedModel.Relevances)
                                {
                                    if (temp.DisplayName == infor.DisplayName)
                                    {
                                        examinedModel.Relevances.Remove(temp);
                                        isSame = true;
                                        break;
                                    }
                                }
                                isSame = true;
                                break;
                            }
                        }
                        if (isSame == false && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            examinedModel.Relevances.Add(new ArticleRelevancesExaminedModel
                            {
                                DisplayName = item.DisplayName,
                                Modifier = "文章",
                                IsDelete = false
                            });
                        }
                    }
                }
                if (model.Groups != null)
                {
                    //循环查找是否相同
                    foreach (var item in model.Groups)
                    {
                        var isSame = false;
                        foreach (var infor in article.Relevances.Where(s => s.Modifier == "制作组"))
                        {
                            if (item.DisplayName == infor.DisplayName)
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in examinedModel.Relevances)
                                {
                                    if (temp.DisplayName == infor.DisplayName)
                                    {
                                        examinedModel.Relevances.Remove(temp);
                                        isSame = true;
                                        break;
                                    }
                                }
                                isSame = true;
                                break;
                            }
                        }
                        if (isSame == false && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            examinedModel.Relevances.Add(new ArticleRelevancesExaminedModel
                            {
                                DisplayName = item.DisplayName,
                                Modifier = "制作组",
                                IsDelete = false
                            });
                        }
                    }
                }
                if (model.Games != null)
                {
                    //循环查找是否相同
                    foreach (var item in model.Games)
                    {
                        var isSame = false;
                        foreach (var infor in article.Relevances.Where(s => s.Modifier == "游戏"))
                        {
                            if (item.DisplayName == infor.DisplayName)
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in examinedModel.Relevances)
                                {
                                    if (temp.DisplayName == infor.DisplayName)
                                    {
                                        examinedModel.Relevances.Remove(temp);
                                        isSame = true;
                                        break;
                                    }
                                }
                                isSame = true;
                                break;
                            }
                        }
                        if (isSame == false && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            examinedModel.Relevances.Add(new ArticleRelevancesExaminedModel
                            {
                                DisplayName = item.DisplayName,
                                Modifier = "游戏",
                                IsDelete = false
                            });
                        }
                    }
                }
                if (model.others != null)
                {
                    //循环查找是否相同
                    foreach (var item in model.others)
                    {
                        foreach (var infor in article.Relevances.Where(s => s.Modifier == "其他"))
                        {
                            if (item.DisplayName == infor.DisplayName)
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in examinedModel.Relevances)
                                {
                                    if (temp.DisplayName == infor.DisplayName && item.DisPlayValue == infor.DisplayValue)
                                    {
                                        examinedModel.Relevances.Remove(temp);
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if (string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            examinedModel.Relevances.Add(new ArticleRelevancesExaminedModel
                            {
                                DisplayName = item.DisplayName,
                                DisplayValue = item.DisPlayValue,
                                Modifier = "其他",
                                Link = item.Link,
                                IsDelete = false
                            });
                        }
                    }
                }


                //判断审核是否为空
                if (examinedModel.Relevances.Count != 0)
                {
                    //序列化JSON
                    var resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, examinedModel);
                        resulte = text.ToString();
                    }
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                    {
                        await _examineService.ExamineEditArticleRelevancesAsync(article, examinedModel);
                        await _examineService.UniversalEditArticleExaminedAsync(article, user, true, resulte, Operation.EditArticleRelevanes, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, article.Id, Operation.EditArticleRelevanes);
                    }
                    else
                    {
                        await _examineService.UniversalEditArticleExaminedAsync(article, user, false, resulte, Operation.EditArticleRelevanes, model.Note);
                    }
                }

                //第三步 修改文章正文

                //判断是否为空
                if (string.IsNullOrWhiteSpace(model.Context) == false && model.Context != article.MainPage)
                {
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                    {
                        await _examineService.ExamineEditArticleMainPageAsync(article, model.Context);
                        await _examineService.UniversalEditArticleExaminedAsync(article, user, true, model.Context, Operation.EditArticleMainPage, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, article.Id, Operation.EditArticleMainPage);

                    }
                    else
                    {
                        await _examineService.UniversalEditArticleExaminedAsync(article, user, false, model.Context, Operation.EditArticleMainPage, model.Note);
                    }
                }


                return new Result { Successful = true, Error = article.Id.ToString() };
            }
            catch
            {
                return new Result { Error = "修改文章的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };

            }

        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenArticleAsync(HiddenArticleModel model)
        {
            await _articleRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();
            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> ThumbsUpArticleAsync(ThumbsUpArticleModel model)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取文章
            var article = await _articleRepository.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (user == null || article == null)
            {
                return NotFound();
            }
            var thumbs = await _thumbsUpRepository.FirstOrDefaultAsync(x => x.ArticleId == model.Id && x.ApplicationUserId == user.Id);

            if (model.IsThumbsUp)
            {
                if (thumbs == null)
                {
                    await _thumbsUpRepository.InsertAsync(new ThumbsUp
                    {
                        ApplicationUserId = user.Id,
                        ApplicationUser = user,
                        Article = article,
                        ArticleId = article.Id,
                        ThumbsUpTime = DateTime.Now.ToCstTime()
                    });


                }
                else
                {
                    return new Result { Successful = false, Error = "当前文章已经被该用户点赞" };
                }
            }
            else
            {
                if (thumbs == null)
                {
                    return new Result { Successful = false, Error = "该用户还没有点赞当前文章" };
                }
                else
                {
                    await _thumbsUpRepository.DeleteAsync(thumbs);
                }

            }
            //计算点赞总数
            var tempCount = await _thumbsUpRepository.CountAsync(s => s.ArticleId == article.Id);
            await _articleRepository.GetRangeUpdateTable().Where(s => s.Id == article.Id).Set(s => s.ThumbsUpCount, b => tempCount).ExecuteAsync();

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditArticlePriorityAsync(EditArticlePriorityViewModel model)
        {
            await _articleRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.Priority, b => b.Priority + model.PlusPriority).ExecuteAsync();

            return new Result { Successful = true };
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<PagedResultDto<ArticleInforTipViewModel>> GetArticleHomeListAsync(PagedSortedAndFilterInput input)
        {
            return await _articleService.GetPaginatedResult(input);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<GetArticleCountModel>> GetArticleCountAsync()
        {
            var model = new GetArticleCountModel
            {
                All = await _articleRepository.CountAsync(),
                Toughts = await _articleRepository.CountAsync(x => x.Type == ArticleType.Tought),
                Interviews = await _articleRepository.CountAsync(x => x.Type == ArticleType.Interview),
                Strategies = await _articleRepository.CountAsync(x => x.Type == ArticleType.Strategy),
                News = await _articleRepository.CountAsync(x => x.Type == ArticleType.News),
                Others = await _articleRepository.CountAsync(x => x.Type == ArticleType.None),
                Hiddens = await _articleRepository.CountAsync(x => x.IsHidden == true)
            };

            return model;
        }

        /// <summary>
        /// 获取输入提示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetAllArticlesAsync()
        {
            return await _articleRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Name).ToArrayAsync();
        }
        /// <summary>
        /// 获取随机词条列表 
        /// </summary>
        /// <returns></returns>
        [HttpGet("{type}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetRandomArticleListViewAsync(ArticleType type)
        {
            var model = new List<EntryHomeAloneViewModel>();
            var length = await _articleRepository.GetAll().AsNoTracking().Where(s => s.Type == type && s.IsHidden != true && s.Name != null && s.Name != "").CountAsync();
            var length_1 = 12;
            List<Article> groups;
            if (length > length_1)
            {
                var random = new Random();
                var temp = random.Next(0, length - length_1);

                groups = await _articleRepository.GetAll().AsNoTracking().Where(s => s.Type == type && s.IsHidden != true && s.Name != null && s.Name != "").Skip(temp).Take(length_1).ToListAsync();
            }
            else
            {
                groups = await _articleRepository.GetAll().AsNoTracking().Where(s => s.Type == type && s.IsHidden != true && s.Name != null && s.Name != "").Take(length_1).ToListAsync();
            }
            foreach (var item in groups)
            {
                model.Add(new EntryHomeAloneViewModel
                {
                    Id = item.Id,
                    Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png"),
                    DisPlayName = item.DisplayName ?? item.Name,
                    CommentCount = item.CommentCount,
                    ReadCount = item.ReaderCount,
                    //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                });
            }
            return model;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetRandomArticleListViewAsync()
        {
            var model = new List<EntryHomeAloneViewModel>();
            var length = await _articleRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && s.Name != null && s.Name != "").CountAsync();
            var length_1 = 12;
            List<Article> groups;
            if (length > length_1)
            {
                var random = new Random();
                var temp = random.Next(0, length - length_1);

                groups = await _articleRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && s.Name != null && s.Name != "").Skip(temp).Take(length_1).ToListAsync();
            }
            else
            {
                groups = await _articleRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && s.Name != null && s.Name != "").Take(length_1).ToListAsync();
            }
            foreach (var item in groups)
            {
                model.Add(new EntryHomeAloneViewModel
                {
                    Id = item.Id,
                    Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png"),
                    DisPlayName = item.DisplayName ?? item.Name,
                    CommentCount = item.CommentCount,
                    ReadCount = item.ReaderCount,
                    //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                });
            }
            return model;
        }
        /// <summary>
        /// 获取所有文章名称ID对
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<NameIdPairModel>>> GetAllArticlesIdNameAsync()
        {
            return await _articleRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => new NameIdPairModel { Name = s.Name, Id = s.Id }).ToArrayAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Result>> RevokeExamine(RevokeExamineModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //查找审核

            await _examineRepository.DeleteRangeAsync(s => s.ArticleId == model.Id && s.ApplicationUserId == user.Id && s.IsPassed == null);
            return new Result { Successful = true };


        }

        [AllowAnonymous]
        [HttpGet("{time}")]
        public async Task<ActionResult<List<NewsSummaryAloneViewModel>>> GetNewSummary(string time)
        {
            //获取所有符合条件的动态
            var days = 0;
            days = time switch
            {
                "本年" => 365,
                "本月" => 30,
                "本周" => 7,
                _ => 9999,
            };
            var nowTime = DateTime.Now.ToCstTime();

            var articles = await _articleRepository.GetAll().Include(s => s.Relevances).Include(s => s.CreateUser)
                .Where(s => s.Type == ArticleType.News && string.IsNullOrWhiteSpace(s.Name) == false && (s.RealNewsTime != null ? (s.RealNewsTime.Value.Date.AddDays(days) > nowTime.Date) : (s.PubishTime.Date.AddDays(days) > nowTime.Date)))
                .OrderByDescending(s => s.RealNewsTime)
                .ThenByDescending(s => s.PubishTime)
                .ToListAsync();

            //查找关联制作组
            var model = new List<NewsSummaryAloneViewModel>();
            foreach (var item in articles)
            {
                var articleInfor = _appHelper.GetArticleInforTipViewModel(item);
                articleInfor.LastEditTime = item.PubishTime;

                var temp = new NewsSummaryAloneViewModel
                {
                    Articles = new List<ArticleInforTipViewModel> { articleInfor }
                };

                var infor = item.Relevances.FirstOrDefault(s => s.Modifier == "制作组");
                if (infor == null)
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
                        temp.GroupImage = string.IsNullOrWhiteSpace(group.Thumbnail) ? _appHelper.GetImagePath(item.CreateUser.PhotoPath, "user.png") : _appHelper.GetImagePath(group.Thumbnail, "user.png");
                        temp.GroupName = group.DisplayName ?? group.Name;
                        temp.GroupId = group.Id;
                    }
                    else
                    {
                        temp.GroupImage = _appHelper.GetImagePath(item.CreateUser.PhotoPath, "user.png");
                        temp.UserId = item.CreateUser.Id;
                        temp.GroupName = item.CreateUser.UserName;
                        temp.GroupId = -1;
                    }
                }
                else
                {
                    temp.GroupImage = _appHelper.GetImagePath(item.CreateUser.PhotoPath, "user.png");
                    temp.UserId = item.CreateUser.Id;
                    temp.GroupName = item.CreateUser.UserName;
                    temp.GroupId = -1;
                }
                model.Add(temp);
            }

            var result = new List<NewsSummaryAloneViewModel>();

            //合并同类项
            while (model.Count > 0)
            {
                var item = model[0];
                var group = model.Where(s => s.GroupName == item.GroupName && s.Articles[0].Id != item.Articles[0].Id).ToList();

                foreach (var item2 in group)
                {
                    item.Articles.Add(item2.Articles[0]);
                }
                result.Add(item);
                model.RemoveAll(s => s.GroupName == item.GroupName);
            }

            return result;

        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<List<ArticleInforTipViewModel>>> GetUserAllArticlesAsync(string id)
        {
            var articles = await _articleRepository.GetAll().Where(s => s.CreateUserId == id).ToListAsync();
            var model = new List<ArticleInforTipViewModel>();
            foreach (var item in articles)
            {
                model.Add(_appHelper.GetArticleInforTipViewModel(item));
            }
            return model;
        }
    }
}
