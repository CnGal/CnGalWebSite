using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Videos;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/videos/[action]")]
    public class VideoAPIController : ControllerBase
    {
        
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<ThumbsUp, long> _thumbsUpRepository;
        private readonly IRepository<Comment, long> _commentUpRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IArticleService _articleService;
        private readonly IEntryService _entryService;
        private readonly IVideoService _videoService;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEditRecordService _editRecordService;
        private readonly ILogger<VideoAPIController> _logger;
        private readonly IQueryService _queryService;

        public VideoAPIController(IArticleService articleService, IRepository<Comment, long> commentUpRepository, IRepository<ThumbsUp, long> thumbsUpRepository, IUserService userService, ILogger<VideoAPIController> logger,
        IExamineService examineService, IEntryService entryService, IRepository<ApplicationUser, string> userRepository, IWebHostEnvironment webHostEnvironment, IEditRecordService editRecordService,
         IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Examine, long> examineRepository, IVideoService videoService,
        IRepository<Entry, int> entryRepository, IRepository<Video, long> videoRepository, IQueryService queryService)
        {
            
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
            _videoRepository = videoRepository;
            _videoService = videoService;
            _logger = logger;
            _queryService = queryService;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<VideoViewModel>> GetViewAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取文章
            var video = await _videoRepository.GetAll().AsNoTracking()
                .Include(s => s.CreateUser)
                .Include(s => s.VideoRelationFromVideoNavigation).ThenInclude(s => s.ToVideoNavigation)
                .Include(s => s.Entries)
                  .Include(s => s.Articles)
                  .Include(s => s.Pictures)
                .Include(s => s.Outlinks)
                .Include(s => s.Examines).ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (video == null)
            {
                return NotFound();
            }


            //判断当前是否隐藏
            if (video.IsHidden == true)
            {
                if (user == null || _userService.CheckCurrentUserRole( "Admin") != true)
                {
                    return NotFound();
                }
            }

            List<Examine> examineQuery = null;


            //读取审核信息
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll().AsNoTracking()
                               .Where(s => s.VideoId == video.Id && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EditVideoImages || s.Operation == Operation.EditVideoMain || s.Operation == Operation.EditVideoMainPage || s.Operation == Operation.EditVideoRelevanes))
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
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EditVideoMain);
                if (examine != null)
                {
                    await _videoService.UpdateData(video, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EditVideoMainPage);

                if (examine != null)
                {
                    await _videoService.UpdateData(video, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EditVideoRelevanes);
                if (examine != null)
                {
                    await _videoService.UpdateData(video, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EditVideoImages);
                if (examine != null)
                {
                    await _videoService.UpdateData(video, examine);
                }
            }

            //建立视图模型
            var model = _videoService.GetViewModel(video);
            model.CommentCount = await _commentUpRepository.CountAsync(s => s.VideoId == id);

            if (user != null)
            {
                if (examineQuery.Any(s => s.Operation == Operation.EditVideoMain))
                {
                    model.MainState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditVideoMainPage))
                {
                    model.MainPageState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditVideoRelevanes))
                {
                    model.RelevancesState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditVideoImages))
                {
                    model.ImagesState = EditState.Preview;
                }
            }

            //复制数据
            var createUser = video.CreateUser;
            if (createUser == null)
            {
                video.CreateUser = createUser = video.Examines.First(s => s.IsPassed == true).ApplicationUser;
            }


            model.UserInfor = await _userService.GetUserInforViewModel(createUser);
            //添加版权Tag
            if(model.IsCreatedByCurrentUser)
            {
                model.UserInfor.Ranks.Add(new DataModel.ViewModel.Ranks.RankViewModel
                {
                    Type = RankType.Rank,
                    Text = "作者",
                    CSS = "bg-success",

                });
            }
            else
            {
                model.UserInfor.Ranks.Add(new DataModel.ViewModel.Ranks.RankViewModel
                {
                    Type = RankType.Rank,
                    Text = "搬运",
                    CSS = "bg-primary",

                });
            }

            //判断是否有权限编辑
            if (user != null && _userService.CheckCurrentUserRole( "Editor") == true)
            {
                model.Authority = true;
            }
            else
            {
                if (user != null && user.Id == video.CreateUser.Id)
                {
                    model.Authority = true;
                }
                else
                {
                    model.Authority = false;
                }
            }

            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.VideoId == video.Id && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }

            //获取各部分状态
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditVideoMain))
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
                    if (examiningList.Any(s => s == Operation.EditVideoMainPage))
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
                    if (examiningList.Any(s => s == Operation.EditVideoRelevanes))
                    {
                        model.RelevancesState = EditState.Locked;
                    }
                    else
                    {
                        model.RelevancesState = EditState.Normal;
                    }
                }
                if (model.ImagesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditVideoImages))
                    {
                        model.ImagesState = EditState.Locked;
                    }
                    else
                    {
                        model.ImagesState = EditState.Normal;
                    }
                }
            }


            //增加阅读次数
            _videoRepository.Clear();
            _ = await _videoRepository.GetAll().Where(s => s.Id == video.Id).ExecuteUpdateAsync(s=>s.SetProperty(s => s.ReaderCount, b => b.ReaderCount + 1));

            return model;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditVideoMainViewModel>> EditMainAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取词条
            var video = await _videoRepository.FirstOrDefaultAsync(s => s.Id == id && s.IsHidden != true);
            if (video == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(id, user.Id, Operation.EditVideoMain))
            {
                return NotFound();
            }

            //获取审核记录
            var examine = await _examineService.GetUserVideoActiveExamineAsync(video.Id, user.Id, Operation.EditVideoMain);
            if (examine != null)
            {
                await _videoService.UpdateData(video, examine);
            }

            var model = _videoService.GetEditMain(video);

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditMainAsync(EditVideoMainViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }

            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(model.Id, user.Id, Operation.EditVideoMain))
            {
                return new Result { Error = "当前词条该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }

            //判断名称是否重复
            if (await _videoRepository.GetAll().AnyAsync(s => s.Name == model.Name && s.Id != model.Id))
            {
                return new Result { Error = "该视频的名称与其他视频重复", Successful = false };
            }

            //查找当前词条
            var currentVideo = await _videoRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            var newVideo = await _videoRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.Id);

            if (currentVideo == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的视频", Successful = false };
            }

            //设置数据
            _videoService.SetDataFromEditMain(newVideo, model);

            var examines = _videoService.ExaminesCompletion(currentVideo, newVideo);

            if (examines.Any(s => s.Value == Operation.EditVideoMain) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EditVideoMain);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentVideo, user, examine.Key, Operation.EditVideoMain, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditVideoImagesViewModel>> EditImagesAsync(long Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var video = await _videoRepository.GetAll().AsNoTracking().Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (video == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(Id, user.Id, Operation.EstablishImages))
            {
                return NotFound();
            }

            //获取用户的审核信息
            var examine = await _examineService.GetUserVideoActiveExamineAsync(video.Id, user.Id, Operation.EstablishImages);
            if (examine != null)
            {
                await _videoService.UpdateData(video, examine);
            }

            return _videoService.GetEditImages(video);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditImagesAsync(EditVideoImagesViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(model.Id, user.Id, Operation.EstablishImages))
            {
                return new Result { Error = "当前词条该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }
            //检查图片链接 是否包含外链
            foreach (var item in model.Images)
            {
                if (item.Image.Contains("image.cngal.org") == false && item.Image.Contains("pic.cngal.top") == false)
                {
                    return new Result { Successful = false, Error = "相册中不能添加外部图片：" + item.Image };
                }
            }
            //检查是否重复
            foreach (var item in model.Images)
            {
                if (model.Images.Count(s => s.Image == item.Image) > 1)
                {
                    return new Result { Error = "图片链接不能重复，重复的链接：" + item.Image, Successful = false };

                }
            }
            //查找词条
            var currentVideo = await _videoRepository.GetAll().Include(s => s.Pictures).FirstOrDefaultAsync(x => x.Id == model.Id);
            var newVideo = await _videoRepository.GetAll().AsNoTracking().Include(s => s.Pictures).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (currentVideo == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的视频", Successful = false };
            }

            //设置数据
            _videoService.SetDataFromEditImages(newVideo, model);

            var examines = _videoService.ExaminesCompletion(currentVideo, newVideo);

            if (examines.Any(s => s.Value == Operation.EditVideoImages) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EditVideoImages);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentVideo, user, examine.Key, Operation.EditVideoImages, model.Note);

            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditVideoRelevancesViewModel>> EditRelevances(long Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var video = await _videoRepository.GetAll().AsNoTracking()
                .Include(s => s.Outlinks)
                .Include(s => s.Articles)
                .Include(s => s.Entries)
                .Include(s => s.VideoRelationFromVideoNavigation).ThenInclude(s => s.ToVideoNavigation)
                .FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);

            if (video == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(Id, user.Id, Operation.EstablishRelevances))
            {
                return NotFound();
            }

            //获取用户的审核信息
            var examine = await _examineService.GetUserVideoActiveExamineAsync(video.Id, user.Id, Operation.EstablishRelevances);
            if (examine != null)
            {
                await _videoService.UpdateData(video, examine);
            }

            return _videoService.GetEditRelevances(video);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditRelevances(EditVideoRelevancesViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(model.Id, user.Id, Operation.EstablishRelevances))
            {
                return new Result { Error = "当前词条该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }
            //查找词条
            var currentVideo = await _videoRepository.GetAll()
              .Include(s => s.Outlinks)
              .Include(s => s.Articles)
                .Include(s => s.Entries)
              .Include(s => s.VideoRelationFromVideoNavigation).ThenInclude(s => s.ToVideoNavigation)
              .FirstOrDefaultAsync(x => x.Id == model.Id);
            var newVideo = await _videoRepository.GetAll().AsNoTracking()
              .Include(s => s.Outlinks)
              .Include(s => s.Articles)
               .Include(s => s.Entries)
              .Include(s => s.VideoRelationFromVideoNavigation).ThenInclude(s => s.ToVideoNavigation)
              .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (currentVideo == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的视频", Successful = false };
            }
            //处理原始数据 删除空项目
            model.Roles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.staffs.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.Groups.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.Games.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.articles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.news.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.videos.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.others.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));


            //预处理 建立词条关联信息
            //判断关联是否存在
            var entryIds = new List<int>();
            var entryNames = new List<string>();

            var articleIds = new List<long>();
            var articleNames = new List<string>();

            var videoIds = new List<long>();
            var videoNames = new List<string>();

            entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            //建立文章关联信息
            articleNames.AddRange(model.articles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            articleNames.AddRange(model.news.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            //建立视频关联信息
            videoNames.AddRange(model.videos.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            try
            {
                entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
                videoIds = await _videoService.GetIdsFromNames(videoNames);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

            //获取词条文章
            var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();
            var articles = await _articleRepository.GetAll().Where(s => articleIds.Contains(s.Id)).ToListAsync();
            var videos = await _videoRepository.GetAll().Where(s => videoIds.Contains(s.Id)).ToListAsync();

            _videoService.SetDataFromEditRelevances(newVideo, model, entries, articles, videos);

            var examines = _videoService.ExaminesCompletion(currentVideo, newVideo);

            if (examines.Any(s => s.Value == Operation.EditVideoRelevanes) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EditVideoRelevanes);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentVideo, user, examine.Key, Operation.EditVideoRelevanes, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditVideoMainPageViewModel>> EditMainPageAsync(long Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var video = await _videoRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (video == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(Id, user.Id, Operation.EditVideoMainPage))
            {
                return NotFound();
            }
            //获取审核记录

            var examine = await _examineService.GetUserVideoActiveExamineAsync(video.Id, user.Id, Operation.EditVideoMainPage);
            if (examine != null)
            {
                await _videoService.UpdateData(video, examine);
            }

            return _videoService.GetEditMainPage(video);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditMainPageAsync(EditVideoMainPageViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(model.Id, user.Id, Operation.EstablishMain))
            {
                return new Result { Error = "当前词条该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }
            //查找词条
            var currentVideo = await _videoRepository.GetAll().FirstOrDefaultAsync(x => x.Id == model.Id);
            var newVideo = await _videoRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.Id);

            if (currentVideo == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的视频", Successful = false };
            }

            _videoService.SetDataFromEditMainPage(newVideo, model);

            var examines = _videoService.ExaminesCompletion(currentVideo, newVideo);

            if (examines.Any(s => s.Value == Operation.EditVideoMainPage) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EditVideoMainPage);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentVideo, user, examine.Key, Operation.EditVideoMainPage, model.Note);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> CreateAsync(CreateVideoViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断名称是否重复
            if (await _videoRepository.FirstOrDefaultAsync(s => s.Name == model.Main.Name) != null)
            {
                return new Result { Error = "该视频的名称与其他视频重复", Successful = false };
            }

            //预处理
            //检查图片链接 是否包含外链
            foreach (var item in model.Images.Images)
            {
                if (item.Image.Contains("image.cngal.org") == false && item.Image.Contains("pic.cngal.top") == false)
                {
                    return new Result { Successful = false, Error = "相册中不能添加外部图片：" + item.Image };
                }
            }
            //检查是否重复
            foreach (var item in model.Images.Images)
            {
                if (model.Images.Images.Count(s => s.Image == item.Image) > 1)
                {
                    return new Result { Error = "图片链接不能重复，重复的链接：" + item.Image, Successful = false };

                }
            }


            //处理原始数据 删除空项目
            model.Relevances.Roles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.Relevances.staffs.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.Relevances.Groups.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.Relevances.Games.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.Relevances.articles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.Relevances.news.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.Relevances.videos.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
            model.Relevances.others.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));


            //预处理 建立词条关联信息
            //判断关联是否存在
            var entryIds = new List<int>();
            var entryNames = new List<string>();

            var articleIds = new List<long>();
            var articleNames = new List<string>();

            var videoIds = new List<long>();
            var videoNames = new List<string>();

            entryNames.AddRange(model.Relevances.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Relevances.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Relevances.staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Relevances.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            //建立文章关联信息
            articleNames.AddRange(model.Relevances.articles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            articleNames.AddRange(model.Relevances.news.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            //建立视频关联信息
            videoNames.AddRange(model.Relevances.videos.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            try
            {
                entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
                videoIds = await _videoService.GetIdsFromNames(videoNames);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

            //获取词条文章
            var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();
            var articles = await _articleRepository.GetAll().Where(s => articleIds.Contains(s.Id)).ToListAsync();
            var vidoes = await _videoRepository.GetAll().Where(s => videoIds.Contains(s.Id)).ToListAsync();


            var newVideo = new Video
            {
                CreateTime = DateTime.Now.ToCstTime()
            };
            _videoService.SetDataFromEditMain(newVideo, model.Main);
            _videoService.SetDataFromEditMainPage(newVideo, model.MainPage);
            _videoService.SetDataFromEditRelevances(newVideo, model.Relevances, entries, articles, vidoes);
            _videoService.SetDataFromEditImages(newVideo, model.Images);

            var video = new Video();
            //获取审核记录
            try
            {
                video = await _examineService.AddNewVideoExaminesAsync(newVideo, user, model.Note);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };

            }

            //创建视频成功
            return new Result { Successful = true, Error = video.Id.ToString() };

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> HideAsync(HiddenArticleModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            await _videoRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));

            foreach (var item in model.Ids)
            {
                _logger.LogInformation("管理员 - {name}({id}){operation}视频 - Id:{entryId}", user.UserName, user.Id, (model.IsHidden ? "隐藏" : "取消隐藏"), item);
            }


            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditPriorityAsync(EditArticlePriorityViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            await _videoRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));
            foreach (var item in model.Ids)
            {
                _logger.LogInformation("管理员 - {name}({id}) 修改视频 - Id:{entryId} 优先级为：{PlusPriority}", user.UserName, user.Id,item, model.PlusPriority);
            }

            return new Result { Successful = true };
        }

        /// <summary>
        /// 获取编辑记录概览
        /// </summary>
        /// <param name="contrastId">要对比的编辑记录</param>
        /// <param name="currentId">当前最新的编辑记录</param>
        /// <returns></returns>
        [HttpGet("{contrastId}/{currentId}")]
        [AllowAnonymous]
        public async Task<ActionResult<VideoContrastEditRecordViewModel>> GetContrastEditRecordViewsAsync(long contrastId, long currentId)
        {
            if (contrastId > currentId)
            {
                return BadRequest("对比的编辑必须先于当前的编辑");
            }

            var contrastExamine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == contrastId);
            var currentExamine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == currentId);

            if (contrastExamine == null || currentExamine == null || contrastExamine.VideoId == null || currentExamine.VideoId == null || contrastExamine.VideoId != currentExamine.VideoId)
            {
                return NotFound("编辑记录Id不正确");
            }

            var currentVideo = new Video();
            var newVideo = new Video();

            //获取审核记录
            var examines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.IsPassed == true && s.VideoId == currentExamine.VideoId).ToListAsync();

            foreach (var item in examines.Where(s => s.Id <= contrastId))
            {
                await _videoService.UpdateData(currentVideo, item);
            }

            foreach (var item in examines.Where(s => s.Id <= currentId))
            {
                await _videoService.UpdateData(newVideo, item);
            }

            var result = _videoService.ConcompareAndGenerateModel(currentVideo, newVideo);

            var model = new VideoContrastEditRecordViewModel
            {
                ContrastId = contrastId,
                CurrentId = currentId,
                ContrastModel = result[0],
                CurrentModel = result[1],
            };

            return model;
        }

        /// <summary>
        /// 获取编辑信息汇总
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EditVideoInforBindModel>> GetEditInforBindModelAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var periphery = await _videoRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (periphery == null)
            {
                return NotFound("无法找到该视频");
            }
            var model = new EditVideoInforBindModel
            {
                Id = id,
                Name = periphery.Name
            };

            //获取编辑记录
            model.Examines = await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => s.VideoId == id && (s.IsPassed == true || (user != null && s.IsPassed == null && s.ApplicationUserId == user.Id))), true);
            model.Examines = model.Examines.OrderByDescending(s => s.ApplyTime).ToList();
            //获取编辑状态
            model.State = await _videoService.GetEditState(user, id);

            return model;
        }

        /// <summary>
        /// 获取输入提示 所有视频名称
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetNamesAsync()
        {
            return await _videoRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Name).ToArrayAsync();
        }

        /// <summary>
        /// 获取输入提示 所有视频类型
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetTypesAsync()
        {
            return (await _videoRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false && string.IsNullOrWhiteSpace(s.Type) == false).Select(s => s.Type).ToListAsync()).Purge();
        }

        /// <summary>
        /// 撤销编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> RevokeExamine(RevokeExamineModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //查找审核
            var examine = await _examineRepository.FirstOrDefaultAsync(s => s.VideoId == model.Id && s.ApplicationUserId == user.Id && s.Operation == model.ExamineType && s.IsPassed == null);
            if (examine != null)
            {
                await _examineRepository.DeleteAsync(examine);
                //删除以此审核为前置审核的
                await _examineRepository.DeleteAsync(s => s.PrepositionExamineId == examine.Id);
                return new Result { Successful = true };
            }
            else
            {
                return new Result { Successful = false, Error = "找不到目标审核记录" };
            }

        }

        [HttpPost]
        public async Task<QueryResultModel<VideoOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Video, long>(_videoRepository.GetAll().AsSingleQuery().Where(s => string.IsNullOrWhiteSpace(s.Name) == false), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<VideoOverviewModel>
            {
                Items = await items.Select(s => new VideoOverviewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsHidden = s.IsHidden,
                    CanComment = s.CanComment,
                    Priority = s.Priority,
                    Type = s.Type,
                    CreateTime = s.CreateTime,
                    LastEditTime = s.LastEditTime
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
