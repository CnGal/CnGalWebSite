using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
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
using CnGalWebSite.Helper.Extensions;
using Markdig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<ThumbsUp, long> _thumbsUpRepository;
        private readonly IRepository<Comment, long> _commentUpRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IArticleService _articleService;
        private readonly IEntryService _entryService;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEditRecordService _editRecordService;

        public ArticlesAPIController(IArticleService articleService, IRepository<Comment, long> commentUpRepository, IRepository<ThumbsUp, long> thumbsUpRepository, IUserService userService,
        IExamineService examineService, IEntryService entryService, IRepository<ApplicationUser, string> userRepository, IWebHostEnvironment webHostEnvironment, IEditRecordService editRecordService,
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
            _entryService = entryService;
            _userService = userService;
            _userRepository = userRepository;
            _webHostEnvironment = webHostEnvironment;
            _editRecordService = editRecordService;
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

            var article = await _articleRepository.GetAll().AsNoTracking()
                .Include(s => s.Entries)
                .Include(s => s.Outlinks)
                .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                .Include(s => s.Examines).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id);

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
                .Include(s => s.CreateUser).Include(s => s.ThumbsUps)
                .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                .Include(s => s.Entries)
                .Include(s => s.Outlinks)
                .Include(s => s.Examines).ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (article == null)
            {
                return NotFound();
            }


            //判断当前是否隐藏
            if (article.IsHidden == true)
            {
                if (user == null || await _userManager.IsInRoleAsync(user, "Admin") != true)
                {
                    return NotFound();
                }
            }

            List<Examine> examineQuery = null;


            //读取审核信息
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll().AsNoTracking()
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
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EditArticleMain);
                if (examine != null)
                {
                    await _articleService.UpdateArticleData(article, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EditArticleMainPage);

                if (examine != null)
                {
                    await _articleService.UpdateArticleData(article, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EditArticleRelevanes);
                if (examine != null)
                {
                    await _articleService.UpdateArticleData(article, examine);
                }
            }

            //建立视图模型
            var model = _articleService.GetArticleViewModelAsync(article);
            model.CommentCount = await _commentUpRepository.LongCountAsync(s => s.ArticleId == id);

            if (user != null)
            {
                if (examineQuery.Any(s => s.Operation == Operation.EditArticleMain))
                {
                    model.MainState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditArticleMain))
                {
                    model.MainPageState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditArticleRelevanes))
                {
                    model.RelevancesState = EditState.Preview;
                }
            }

            //复制数据
            var createUser = article.CreateUser;
            if (createUser == null)
            {
                article.CreateUser = createUser = article.Examines.First(s => s.IsPassed == true).ApplicationUser;
            }


            model.UserInfor = await _userService.GetUserInforViewModel(createUser);
            model.LastExamineId = article.Examines.Last().Id;

            //判断是否有权限编辑
            if (user != null && await _userManager.IsInRoleAsync(user, "Editor") == true)
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
                    await _articleService.UpdateArticleData(article, examine);
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
                        model.MainState = EditState.Locked;
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
                        model.MainPageState = EditState.Locked;
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
                        model.RelevancesState = EditState.Locked;
                    }
                    else
                    {
                        model.RelevancesState = EditState.Normal;
                    }
                }
            }


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
                if (user == null || await _userManager.IsInRoleAsync(user, "Editor") != true)
                {
                    return NotFound();
                }
            }
            //判断是否有权限编辑
            if (user != null && await _userManager.IsInRoleAsync(user, "Editor") == true)
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
                if (await _articleRepository.FirstOrDefaultAsync(s => s.Name == model.Main.Name) != null)
                {
                    return new Result { Error = "该文章的名称与其他文章重复", Successful = false };
                }

                if (model.Main.Type == ArticleType.Notice && await _userManager.IsInRoleAsync(user, "Admin") == false)
                {
                    return new Result { Error = "只有管理员才有权限发布公告", Successful = false };
                }

                //处理原始数据 删除空项目
                model.Relevances.Roles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.Staffs.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.Groups.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.Games.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.Articles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.Others.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));


                //预处理 建立词条关联信息
                //判断关联是否存在
                var entryIds = new List<int>();
                var entryNames = new List<string>();

                var articleIds = new List<long>();
                var articleNames = new List<string>();

                entryNames.AddRange(model.Relevances.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Relevances.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Relevances.Staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Relevances.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

                //建立文章关联信息
                articleNames.AddRange(model.Relevances.Articles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

                try
                {
                    entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                    articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
                }
                catch (Exception ex)
                {
                    return new Result { Successful = false, Error = ex.Message };
                }
                var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();
                var articles = await _articleRepository.GetAll().Where(s => articleIds.Contains(s.Id)).ToListAsync();


                var newArticle = new Article
                {
                    CreateTime = DateTime.Now.ToCstTime()
                };

                _articleService.SetDataFromEditArticleMainViewModel(newArticle, model.Main);
                _articleService.SetDataFromEditArticleMainPageViewModel(newArticle, model.MainPage);
                _articleService.SetDataFromEditArticleRelevancesViewModel(newArticle, model.Relevances, entries, articles);

                var article = new Article();
                //获取审核记录
                try
                {
                    article = await _examineService.AddNewArticleExaminesAsync(newArticle, user, model.Note);
                }
                catch (Exception ex)
                {
                    return new Result { Successful = false, Error = ex.Message };

                }
                return new Result { Successful = true, Error = article.Id.ToString() };
            }
            catch
            {
                return new Result { Error = "发表文章的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditArticleMainViewModel>> EditArticleMainAsync(long Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var article = await _articleRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == Id);
            if (article == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && s.ArticleId == Id
            && (s.Operation == Operation.EditArticleMain)))
            {
                return NotFound();
            }

            var model = new EditArticleMainViewModel
            {
                Id = article.Id,
                Name = article.Name,
            };
            //获取审核记录
            var examines = await _examineRepository.GetAllListAsync(s => s.ArticleId == article.Id && s.ApplicationUserId == user.Id
              && (s.Operation == Operation.EditArticleMain) && s.IsPassed == null);

            //第一步 获取主要信息
            var examine = examines.FirstOrDefault(s => s.Operation == Operation.EditArticleMain);
            if (examine != null)
            {
                await _articleService.UpdateArticleData(article, examine);
            }
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


            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditArticleMainAsync(EditArticleMainViewModel model)
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
                if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId != user.Id && s.ArticleId == model.Id && s.IsPassed == null && (s.Operation == Operation.EditArticleMain)))
                {
                    return new Result { Error = "当前文章已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
                }
                if (model.Type == ArticleType.Notice && await _userManager.IsInRoleAsync(user, "Admin") == false)
                {
                    return new Result { Error = "只有管理员才有权限发布公告", Successful = false };
                }

                //判断名称是否重复
                if (await _articleRepository.GetAll().AnyAsync(s => s.Name == model.Name && s.Id != model.Id))
                {
                    return new Result { Error = "该文章的名称与其他文章重复", Successful = false };
                }



                //查找当前文章
                var currentArticle = await _articleRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.Id == model.Id);
                var newArticle = await _articleRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == model.Id);

                if (currentArticle == null)
                {
                    return new Result { Error = $"无法找到ID为{model.Id}的文章", Successful = false };
                }



                //第一步 修改文章主要信息

                _articleService.SetDataFromEditArticleMainViewModel(newArticle, model);



                var examines = _articleService.ExaminesCompletion(currentArticle, newArticle);

                if (examines.Any(s => s.Value == Operation.EditArticleMain) == false)
                {
                    return new Result { Successful = true };
                }
                var examine = examines.FirstOrDefault(s => s.Value == Operation.EditArticleMain);

                //保存并尝试应用审核记录
                await _editRecordService.SaveAndApplyEditRecord(currentArticle, user, examine.Key, Operation.EditArticleMain, model.Note);

                return new Result { Successful = true, Error = currentArticle.Id.ToString() };
            }
            catch
            {
                return new Result { Error = "修改文章的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };

            }

        }


        [HttpGet("{id}")]
        public async Task<ActionResult<EditArticleMainPageViewModel>> EditArticleMainPageAsync(long Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取词条
            var article = await _articleRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == Id);
            if (article == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && s.ArticleId == Id
            && (s.Operation == Operation.EditArticleMainPage)))
            {
                return NotFound();
            }

            var model = new EditArticleMainPageViewModel
            {
                Id = article.Id,
                Name = article.Name,
            };
            //获取审核记录
            var examines = await _examineRepository.GetAllListAsync(s => s.ArticleId == article.Id && s.ApplicationUserId == user.Id
              && (s.Operation == Operation.EditArticleMainPage) && s.IsPassed == null);

            //第三步 获取正文
            var examine = examines.FirstOrDefault(s => s.Operation == Operation.EditArticleMainPage);
            if (examine != null)
            {
                await _articleService.UpdateArticleData(article, examine);
            }


            model.Context = article.MainPage;

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditArticleMainPageAsync(EditArticleMainPageViewModel model)
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
                if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId != user.Id && s.ArticleId == model.Id && s.IsPassed == null && (s.Operation == Operation.EditArticleMainPage)))
                {
                    return new Result { Error = "当前文章已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
                }

                //判断名称是否重复
                if (await _articleRepository.GetAll().AnyAsync(s => s.Name == model.Name && s.Id != model.Id))
                {
                    return new Result { Error = "该文章的名称与其他文章重复", Successful = false };
                }


                //查找当前文章
                var currentArticle = await _articleRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.Id == model.Id);
                var newArticle = await _articleRepository.GetAll().AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == model.Id);

                if (currentArticle == null)
                {
                    return new Result { Error = $"无法找到ID为{model.Id}的文章", Successful = false };
                }




                //第三步 修改文章正文
                _articleService.SetDataFromEditArticleMainPageViewModel(newArticle, model);


                var examines = _articleService.ExaminesCompletion(currentArticle, newArticle);

                if (examines.Any(s => s.Value == Operation.EditArticleMainPage) == false)
                {
                    return new Result { Successful = true };
                }
                var examine = examines.FirstOrDefault(s => s.Value == Operation.EditArticleMainPage);

                //保存并尝试应用审核记录
                await _editRecordService.SaveAndApplyEditRecord(currentArticle, user, examine.Key, Operation.EditArticleMainPage, model.Note);


                return new Result { Successful = true, Error = currentArticle.Id.ToString() };
            }
            catch
            {
                return new Result { Error = "修改文章的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditArticleRelevancesViewModel>> EditArticleRelevancesAsync(long Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取词条
            var article = await _articleRepository.GetAll().AsNoTracking()
                .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                .Include(s => s.Entries)
                .Include(s => s.Outlinks)
                .FirstOrDefaultAsync(s => s.Id == Id);
            if (article == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && s.ArticleId == Id
            && (s.Operation == Operation.EditArticleRelevanes)))
            {
                return NotFound();
            }

            var model = new EditArticleRelevancesViewModel
            {
                Id = article.Id,
                Name = article.Name,
            };
            //获取审核记录
            var examines = await _examineRepository.GetAllListAsync(s => s.ArticleId == article.Id && s.ApplicationUserId == user.Id
              && (s.Operation == Operation.EditArticleRelevanes) && s.IsPassed == null);


            var examine = examines.FirstOrDefault(s => s.Operation == Operation.EditArticleRelevanes);
            if (examine != null)
            {
                await _articleService.UpdateArticleData(article, examine);
            }

            var roles = new List<RelevancesModel>();
            var staffs = new List<RelevancesModel>();
            var articles = new List<RelevancesModel>();
            var groups = new List<RelevancesModel>();
            var games = new List<RelevancesModel>();
            var news = new List<RelevancesModel>();
            var others = new List<RelevancesModel>();

            foreach (var nav in article.ArticleRelationFromArticleNavigation)
            {
                var item = nav.ToArticleNavigation;

                articles.Add(new RelevancesModel
                {
                    DisplayName = item.Name
                });

            }
            foreach (var item in article.Entries)
            {
                switch (item.Type)
                {
                    case EntryType.Role:
                        roles.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    case EntryType.Staff:
                        staffs.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    case EntryType.ProductionGroup:
                        groups.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    case EntryType.Game:
                        games.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                }
            }
            foreach (var item in article.Outlinks)
            {
                others.Add(new RelevancesModel
                {
                    DisplayName = item.Name,
                    DisPlayValue = item.BriefIntroduction,
                    Link = item.Link
                });
            }

            model.Roles = roles;
            model.Staffs = staffs;
            model.Articles = articles;
            model.Groups = groups;
            model.Games = games;
            model.Others = others;

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditArticleRelevancesAsync(EditArticleRelevancesViewModel model)
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

                //处理原始数据 删除空项目
                model.Roles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Staffs.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Groups.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Games.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Articles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Others.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));


                //预处理 建立词条关联信息
                //判断关联是否存在
                var entryIds = new List<int>();
                var entryNames = new List<string>();

                var articleIds = new List<long>();
                var articleNames = new List<string>();

                entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

                //建立文章关联信息
                articleNames.AddRange(model.Articles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

                try
                {
                    entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                    articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
                }
                catch (Exception ex)
                {
                    return new Result { Successful = false, Error = ex.Message };
                }

                //查找当前文章
                var currentArticle = await _articleRepository.GetAll()
                    .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                    .Include(s => s.Entries)
                    .Include(s => s.Outlinks)
                    .FirstOrDefaultAsync(s => s.Id == model.Id);
                var newArticle = await _articleRepository.GetAll().AsNoTracking()
                    .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                    .Include(s => s.Entries)
                    .Include(s => s.Outlinks)
                    .FirstOrDefaultAsync(s => s.Id == model.Id);

                if (currentArticle == null)
                {
                    return new Result { Error = $"无法找到ID为{model.Id}的文章", Successful = false };
                }



                var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();
                var articles = await _articleRepository.GetAll().Where(s => articleIds.Contains(s.Id)).ToListAsync();

                _articleService.SetDataFromEditArticleRelevancesViewModel(newArticle, model, entries, articles);

                var examines = _articleService.ExaminesCompletion(currentArticle, newArticle);

                if (examines.Any(s => s.Value == Operation.EditArticleRelevanes) == false)
                {
                    return new Result { Successful = true };
                }
                var examine = examines.FirstOrDefault(s => s.Value == Operation.EditArticleRelevanes);

                //保存并尝试应用审核记录
                await _editRecordService.SaveAndApplyEditRecord(currentArticle, user, examine.Key, Operation.EditArticleRelevanes, model.Note);

                return new Result { Successful = true, Error = currentArticle.Id.ToString() };
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
                _ => 99999,
            };
            var nowTime = DateTime.Now.ToCstTime();

            var articles = await _articleRepository.GetAll()
                .Include(s => s.Entries)
                .Include(s => s.CreateUser)
                .Where(s => s.Type == ArticleType.News && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false && (s.RealNewsTime != null ? (s.RealNewsTime.Value.Date.AddDays(days) > nowTime.Date) : (s.PubishTime.Date.AddDays(days) > nowTime.Date)))
                .OrderByDescending(s => s.PubishTime)
                .ThenByDescending(s => s.RealNewsTime)
                .ToListAsync();

            //查找关联制作组
            var model = new List<NewsSummaryAloneViewModel>();
            foreach (var item in articles)
            {
                var articleInfor = _appHelper.GetArticleInforTipViewModel(item);
                articleInfor.LastEditTime = item.PubishTime;

                var infor = await _articleService.GetNewsModelAsync(item);
                var temp = new NewsSummaryAloneViewModel
                {
                    GroupId = infor.GroupId,
                    UserId = infor.UserId,
                    Articles = new List<ArticleInforTipViewModel> { articleInfor },
                    GroupImage = infor.Image,
                    GroupName = infor.GroupName,
                };

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
            //查找每一组的微博链接和简介

            var userIds = result.Where(s => string.IsNullOrWhiteSpace(s.UserId) == false).Select(s => s.UserId);
            var users = await _userRepository.GetAll().Where(s => userIds.Contains(s.Id)).Select(s => new
            {
                s.Id,
                s.PersonalSignature
            }).ToListAsync();

            var entryIds = result.Where(s => s.GroupId > 0).Select(s => s.GroupId);
            var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).Select(s => new
            {
                s.Id,
                s.BriefIntroduction,
                Informations = s.Information.Where(s => s.Modifier == "相关网站" && (s.DisplayName == "微博" || s.DisplayName == "Bilibili"))
            }).ToListAsync();


            foreach (var item in result)
            {
                if (item.GroupId != 0)
                {
                    var temp = entries.FirstOrDefault(s => s.Id == item.GroupId);
                    if (temp != null)
                    {
                        item.Outlink = temp.Informations.FirstOrDefault()?.DisplayValue;
                        item.BriefIntroduction = temp.BriefIntroduction;
                    }

                }
                else if (string.IsNullOrWhiteSpace(item.UserId) == false)
                {
                    var temp = users.FirstOrDefault(s => s.Id == item.UserId);
                    if (temp != null)
                    {
                        item.BriefIntroduction = temp.PersonalSignature;
                    }
                }
            }

            //            StringBuilder sb = new StringBuilder();
            //            sb.AppendLine("名称,最后发布时间,发布总数,微博链接,B站链接,站内链接");
            //            foreach (var item in result)
            //            {
            //                var weibo = entries.FirstOrDefault(s => s.Id == item.GroupId)?.Informations.FirstOrDefault(s => s.DisplayName == "微博")?.DisplayValue;
            //                var bilibili = entries.FirstOrDefault(s => s.Id == item.GroupId)?.Informations.FirstOrDefault(s => s.DisplayName == "Bilibili")?.DisplayValue;
            //                sb.AppendLine($"{item.GroupName},{item.Articles.Max(s => s.LastEditTime)},{item.Articles.Count},{weibo},{bilibili},{(item.GroupId > 0 ? ($"https://www.cngal.org/entries/index/{item.GroupId}") : ($"https://www.cngal.org/space/index/{item.UserId}"))}");
            //}

            //            var str=sb.ToString();
            //            using StreamWriter sw = new StreamWriter(Path.Combine(_webHostEnvironment.WebRootPath, "BackUp", "当前数据", "制作组导出.csv"),false, Encoding.UTF8);//这里写上你要保存的路径
            //            sw.WriteLine(str);//按行写
            //            sw.Close();//关闭

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

        /// <summary>
        /// 获取编辑记录概览
        /// </summary>
        /// <param name="contrastId">要对比的编辑记录</param>
        /// <param name="currentId">当前最新的编辑记录</param>
        /// <returns></returns>
        [HttpGet("{contrastId}/{currentId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ArticleContrastEditRecordViewModel>> GetContrastEditRecordViewsAsync(long contrastId, long currentId)
        {
            if (contrastId > currentId)
            {
                return BadRequest("对比的编辑必须先于当前的编辑");
            }

            var contrastExamine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == contrastId);
            var currentExamine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == currentId);

            if (contrastExamine == null || currentExamine == null || contrastExamine.ArticleId == null || currentExamine.ArticleId == null || contrastExamine.ArticleId != currentExamine.ArticleId)
            {
                return NotFound("编辑记录Id不正确");
            }

            var currentArticle = new Article();
            var newArticle = new Article();

            //获取审核记录
            var examines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.IsPassed == true && s.ArticleId == currentExamine.ArticleId).ToListAsync();

            foreach (var item in examines.Where(s => s.Id <= contrastId))
            {
                await _articleService.UpdateArticleData(currentArticle, item);
            }

            foreach (var item in examines.Where(s => s.Id <= currentId))
            {
                await _articleService.UpdateArticleData(newArticle, item);
            }

            var result = _articleService.ConcompareAndGenerateModel(currentArticle, newArticle);

            var model = new ArticleContrastEditRecordViewModel
            {
                ContrastId = contrastId,
                CurrentId = currentId,
                ContrastModel = result[0],
                CurrentModel = result[1],
            };

            return model;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EditArticleInforBindModel>> GetArticleEditInforBindModelAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var periphery = await _articleRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (periphery == null)
            {
                return NotFound("无法找到该文章");
            }
            var model = new EditArticleInforBindModel
            {
                Id = id,
                Name = periphery.Name
            };

            //获取编辑记录
            model.Examines = await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => s.ArticleId == id && (s.IsPassed == true || (user != null && s.IsPassed == null && s.ApplicationUserId == user.Id))), true);
            model.Examines = model.Examines.OrderByDescending(s => s.ApplyTime).ToList();
            //获取编辑状态
            model.State = await _articleService.GetArticleEditState(user, id);

            return model;
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<GameEvaluationsModel>>> GetGameEvaluationsAsync()
        {
            var entryIds = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Articles).ThenInclude(s => s.CreateUser)
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.Articles.Count(s => s.Type == ArticleType.Evaluation) > 0)
                .Select(s => s.Id)
                .ToListAsync();

            entryIds = entryIds.Random().Take(20).ToList();

            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Articles).ThenInclude(s => s.CreateUser)
                .Where(s=> entryIds.Contains(s.Id))
                 .Select(s => new
                 {
                     s.Id,
                     s.DisplayName,
                     Articles = s.Articles.Where(s => s.Type == ArticleType.Evaluation)
                 })
                .ToListAsync();

            var model = new List<GameEvaluationsModel>();
            //初始化主页Html代码
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();

            foreach (var item in entries)
            {
                var temp = new GameEvaluationsModel
                {
                    Name = item.DisplayName,
                    Id = item.Id
                };
                foreach (var infor in item.Articles)
                {
                    var infor1 = _appHelper.GetArticleInforTipViewModel(infor);
                    infor1.BriefIntroduction = Markdown.ToHtml(infor.MainPage ?? "", pipeline);

                    temp.Evaluations.Add(infor1);
                }

                model.Add(temp);
            }

            return model;
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ArticleInforTipViewModel>>> GetRandomArticlesAsync()
        {
            var articleIds = await _articleRepository.GetAll()
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && string.IsNullOrWhiteSpace(s.MainPicture) == false
                        && (s.Type != ArticleType.News && s.Type != ArticleType.Evaluation))
                .Select(s=>s.Id)
                .ToListAsync();

            articleIds = articleIds.Random().Take(40).ToList();

            var articles = await _articleRepository.GetAll()
                .Where(s => articleIds.Contains(s.Id))
                .ToListAsync();

            var model = new List<ArticleInforTipViewModel>();
            foreach (var item in articles)
            {
                model.Add(_appHelper.GetArticleInforTipViewModel(item));
            }

            return model;
        }
    }
}
