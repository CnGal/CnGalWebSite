using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Videos;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/entries/[action]")]
    public class EntriesAPIController : ControllerBase
    {
        
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IRepository<EntryInformationType, long> _entryInformationTypeRepository;
        private readonly IAppHelper _appHelper;
        private readonly IEntryService _entryService;
        private readonly IArticleService _articleService;
        private readonly IVideoService _videoService;
        private readonly IExamineService _examineService;
        private readonly IPerfectionService _perfectionService;
        private readonly IEditRecordService _editRecordService;
        private readonly ILogger<EntriesAPIController> _logger;
        private readonly IUserService _userService;
        private readonly IQueryService _queryService;

        public EntriesAPIController( IRepository<Article, long> articleRepository, IRepository<Periphery, long> peripheryRepository, IVideoService videoService, IRepository<Lottery, long> lotteryRepository,
        IPerfectionService perfectionService, IRepository<Examine, long> examineRepository, IArticleService articleService, IEditRecordService editRecordService, ILogger<EntriesAPIController> logger, IUserService userService,
        IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Tag, int> tagRepository, IEntryService entryService, IExamineService examineService, IRepository<Video, long> videoRepository, IQueryService queryService,
        IRepository<EntryInformationType, long> entryInformationTypeRepository)
        {
            
            _entryRepository = entryRepository;
            _tagRepository = tagRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _examineRepository = examineRepository;
            _entryService = entryService;
            _examineService = examineService;
            _perfectionService = perfectionService;
            _articleService = articleService;
            _peripheryRepository = peripheryRepository;
            _editRecordService = editRecordService;
            _logger = logger;
            _videoService = videoService;
            _videoRepository = videoRepository;
            _lotteryRepository = lotteryRepository;
            _userService = userService;
            _queryService = queryService;
            _entryInformationTypeRepository = entryInformationTypeRepository;
        }

        /// <summary>
        /// 通过Id获取为页面显示优化后的词条信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EntryIndexViewModel>> GetEntryViewAsync(int id, [FromQuery] bool renderMarkdown = true)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取词条 
            var entry = await _entryRepository.GetAll()
                    .Include(s => s.Outlinks)
                    .Include(s=>s.Audio)
                    .Include(s => s.WebsiteAddInfor).ThenInclude(s => s.Images)
                    .Include(s => s.Booking).ThenInclude(s=>s.Goals)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Audio)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information)
                    .Include(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryStaffToEntryNavigation).ThenInclude(s => s.FromEntryNavigation)                  
                    .Include(s => s.Articles).ThenInclude(s => s.CreateUser)
                    .Include(s => s.Videos).ThenInclude(s => s.CreateUser)
                    .Include(s => s.Articles).ThenInclude(s => s.Entries)
                    .Include(s => s.Information)
                    .Include(s => s.Tags)
                    .Include(s => s.Pictures)
                    .Include(s=>s.Releases)
                    .FirstOrDefaultAsync(x => x.Id == id);


            if (entry == null)
            {
                return NotFound();
            }
            //判断当前是否隐藏
            if (entry.IsHidden == true)
            {
                if (user == null || _userService.CheckCurrentUserRole( "Editor") != true)
                {
                    return NotFound();
                }
            }

            //读取审核信息
            List<Examine> examineQuery = null;
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll().AsNoTracking()
                               .Where(s => s.EntryId == entry.Id && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EstablishMain || s.Operation == Operation.EstablishMainPage || s.Operation == Operation.EstablishAddInfor || s.Operation == Operation.EstablishImages
                               || s.Operation == Operation.EstablishRelevances || s.Operation == Operation.EstablishTags || s.Operation == Operation.EstablishAudio || s.Operation == Operation.EstablishWebsite))
                               .Select(s => new Examine
                               {
                                   Operation = s.Operation,
                                   Context = s.Context
                               })
                               .ToListAsync();
            }

            if (user != null)
            {
                var examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EstablishMain);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EstablishMainPage);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EstablishAddInfor);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EstablishImages);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EstablishRelevances);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EstablishTags);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EstablishAudio);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
                examine = examineQuery.FirstOrDefault(s => s.Operation == Operation.EstablishWebsite);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
            }

            //建立视图模型
            _entryRepository.Clear();
            var model = await _entryService.GetEntryIndexViewModelAsync(entry, renderMarkdown);

            if (user != null)
            {
                if (examineQuery.Any(s => s.Operation == Operation.EstablishMain))
                {
                    model.MainState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishMainPage))
                {
                    model.MainPageState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishAddInfor))
                {
                    model.InforState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishImages))
                {
                    model.ImagesState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishRelevances))
                {
                    model.RelevancesState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishTags))
                {
                    model.TagState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishAudio))
                {
                    model.AudioState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishWebsite))
                {
                    model.WebsiteState = EditState.Preview;
                }
            }

            //获取各部分状态
            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.EntryId == entry.Id && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();
            }
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EstablishMain))
                    {
                        model.MainState = EditState.Locked;
                    }
                    else
                    {
                        model.MainState = EditState.Normal;
                    }
                }
                if (model.InforState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EstablishAddInfor))
                    {
                        model.InforState = EditState.Locked;
                    }
                    else
                    {
                        model.InforState = EditState.Normal;
                    }
                }
                if (model.MainPageState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EstablishMainPage))
                    {
                        model.MainPageState = EditState.Locked;
                    }
                    else
                    {
                        model.MainPageState = EditState.Normal;
                    }
                }
                if (model.ImagesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EstablishImages))
                    {
                        model.ImagesState = EditState.Locked;
                    }
                    else
                    {
                        model.ImagesState = EditState.Normal;
                    }
                }
                if (model.RelevancesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EstablishRelevances))
                    {
                        model.RelevancesState = EditState.Locked;
                    }
                    else
                    {
                        model.RelevancesState = EditState.Normal;
                    }
                }
                if (model.TagState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EstablishTags))
                    {
                        model.TagState = EditState.Locked;
                    }
                    else
                    {
                        model.TagState = EditState.Normal;
                    }
                }
                if (model.AudioState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EstablishAudio))
                    {
                        model.AudioState = EditState.Locked;
                    }
                    else
                    {
                        model.AudioState = EditState.Normal;
                    }
                }
                if (model.WebsiteState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EstablishAudio))
                    {
                        model.WebsiteState = EditState.Locked;
                    }
                    else
                    {
                        model.WebsiteState = EditState.Normal;
                    }
                }
            }

            //增加词条阅读次数
            
            await _appHelper.EntryReaderNumUpAsync(entry.Id);

            return model;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<PagedResultDto<EntryInforTipViewModel>> GetEntryHomeListAsync(PagedSortedAndFilterInput input)
        {
            return await _entryService.GetPaginatedResult(input);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditMainViewModel>> EditMainAsync(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取词条
            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == id && s.IsHidden != true);
            if (entry == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(id, user.Id, Operation.EstablishMain))
            {
                return NotFound();
            }

            //获取审核记录
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishMain);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            var model = _entryService.GetEditMainViewModel(entry);

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditMainAsync(EditMainViewModel model)
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
            //判断名称是否重复
            if (await _entryRepository.GetAll().AnyAsync(s => s.Name == model.Name && s.Id != model.Id))
            {
                return new Result { Error = "该词条的名称与其他词条重复", Successful = false };
            }

            //查找当前词条
            var currentEntry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.Id);

            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }

            //设置数据
            _entryService.SetDataFromEditMainViewModel(newEntry, model);

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishMain) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishMain);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentEntry, user, examine.Key, Operation.EstablishMain, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditAddInforViewModel>> EditAddInforAsync(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking()
                .Include(s=>s.Booking).ThenInclude(s=>s.Goals)
                .Include(s => s.Information)
                .Include(s=>s.EntryStaffFromEntryNavigation).ThenInclude(s=>s.ToEntryNavigation)
                .Include(s=>s.Releases)
                .FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (entry == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(entry.Id, user.Id, Operation.EstablishAddInfor))
            {
                return NotFound();
            }
            //获取审核记录
            //根据类别生成首个视图模型

            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishAddInfor);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }
            examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishMain);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            return await _entryService.GetEditAddInforViewModel(entry);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditAddInforAsync(EditAddInforViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(model.Id, user.Id, Operation.EstablishAddInfor))
            {
                return new Result { Error = "当前词条该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }

            //查找词条
            var currentEntry = await _entryRepository.GetAll()
                .Include(s => s.Booking).ThenInclude(s => s.Goals)
                .Include(s => s.Information)
                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.Releases)
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Booking).ThenInclude(s => s.Goals)
                .Include(s => s.Information)
                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.Releases)
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }
            //检查预约关联抽奖是否存在
            int lotteryId = 0;
            if(string.IsNullOrWhiteSpace( model.Booking.LotteryName)==false)
            {
                var lottery = await _lotteryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden == false && s.Name == model.Booking.LotteryName).FirstOrDefaultAsync();
                if(lottery==null)
                {
                    return new Result { Successful = false, Error = "预约关联抽奖不存在" };
                }
                else
                {
                    lotteryId =(int) lottery.Id;
                }
            }


            //设置数据
            await _entryService.SetDataFromEditAddInforViewModelAsync(newEntry, model, lotteryId);

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishAddInfor) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishAddInfor);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentEntry, user, examine.Key, Operation.EstablishAddInfor, model.Note);

            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditImagesViewModel>> EditImagesAsync(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (entry == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(Id, user.Id, Operation.EstablishImages))
            {
                return NotFound();
            }

            //获取用户的审核信息
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishImages);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            return _entryService.GetEditImagesViewModel(entry);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditImagesAsync(EditImagesViewModel model)
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
            var currentEntry = await _entryRepository.GetAll().Include(s => s.Pictures).FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Pictures).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }

            //设置数据
            _entryService.SetDataFromEditImagesViewModel(newEntry, model);

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishImages) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishImages);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentEntry, user, examine.Key, Operation.EstablishImages, model.Note);



            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditRelevancesViewModel>> EditRelevances(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Outlinks)
                .Include(s => s.Articles)
                .Include(s => s.Videos)
                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (entry == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(Id, user.Id, Operation.EstablishRelevances))
            {
                return NotFound();
            }

            //获取用户的审核信息
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishRelevances);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }
            examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishMain);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            return _entryService.GetEditRelevancesViewModel(entry);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditRelevances(EditRelevancesViewModel model)
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
            var currentEntry = await _entryRepository.GetAll()
              .Include(s => s.Outlinks)
              .Include(s => s.Articles)
              .Include(s => s.Videos)
              .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
              .FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking()
              .Include(s => s.Outlinks)
              .Include(s => s.Videos)
              .Include(s => s.Articles)
              .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
              .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
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

            var videoIds = new List<long>();
            var videoNames = new List<string>();

            var articleIds = new List<long>();
            var articleNames = new List<string>();

            entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            //建立文章关联信息
            articleNames.AddRange(model.articles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            articleNames.AddRange(model.news.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            //视频
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

            _entryService.SetDataFromEditRelevancesViewModel(newEntry, model, entries, articles,videos);

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishRelevances) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishRelevances);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentEntry, user, examine.Key, Operation.EstablishRelevances, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditMainPageViewModel>> EditMainPageAsync(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (entry == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(Id, user.Id, Operation.EstablishMainPage))
            {
                return NotFound();
            }
            //获取审核记录

            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishMainPage);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            return _entryService.GetEditMainPageViewModel(entry);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditMainPageAsync(EditMainPageViewModel model)
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
            var currentEntry = await _entryRepository.GetAll().FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.Id);

            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }

            _entryService.SetDataFromEditMainPageViewModel(newEntry, model);

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishMainPage) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishMainPage);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentEntry, user, examine.Key, Operation.EstablishMainPage, model.Note);

            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditEntryTagViewModel>> EditTags(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取词条
            var entry = await _entryRepository.GetAll().Include(s => s.Tags).FirstOrDefaultAsync(s => s.Id == Id);
            if (entry == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(Id, user.Id, Operation.EstablishTags))
            {
                return NotFound();
            }

            //获取用户审核记录
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishTags);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            return _entryService.GetEditTagsViewModel(entry);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditTagsAsync(EditEntryTagViewModel model)
        {
            //如果列表为空则进行创建
            if (model.Tags == null)
            {
                model.Tags = new List<RelevancesModel>();
            }


            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(model.Id, user.Id, Operation.EstablishTags))
            {
                return new Result { Error = "当前词条该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }
            //查找词条
            var currentEntry = await _entryRepository.GetAll().Include(s => s.Tags).FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Tags).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }

            //预处理 建立词条关联信息
            //判断关联是否存在
            var tags = new List<Tag>();

            var tagNames = new List<string>();
            tagNames.AddRange(model.Tags.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            tagNames = tagNames.Distinct().ToList();

            foreach (var item in tagNames)
            {
                var infor = await _tagRepository.GetAll().Where(s => s.Name == item).FirstOrDefaultAsync();
                if (infor == null)
                {
                    return new Result { Successful = false, Error = "标签 " + item + " 不存在" };
                }
                else
                {
                    tags.Add(infor);
                }
            }
            _entryService.SetDataFromEditTagsViewModel(newEntry, model, tags);

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishTags) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishTags);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentEntry, user, examine.Key, Operation.EstablishTags, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditAudioViewModel>> EditAudioAsync(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Audio).FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (entry == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(Id, user.Id, Operation.EstablishAudio))
            {
                return NotFound();
            }

            //获取用户的审核信息
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishAudio);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            return _entryService.GetEditAudioViewModel(entry);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditAudioAsync(EditAudioViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(model.Id, user.Id, Operation.EstablishAudio))
            {
                return new Result { Error = "当前词条该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }
            //检查是否重复
            foreach (var item in model.Audio)
            {
                if (model.Audio.Count(s => s.Url == item.Url) > 1)
                {
                    return new Result { Error = $"{item.Name} 与其他音频重复了，链接：{item.Url}", Successful = false };

                }
            }
            //查找词条
            var currentEntry = await _entryRepository.GetAll().Include(s => s.Audio).FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Audio).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }

            //设置数据
            _entryService.SetDataFromEditAudioViewModel(newEntry, model);

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishAudio) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishAudio);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentEntry, user, examine.Key, Operation.EstablishAudio, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditEntryWebsiteViewModel>> EditWebsiteAsync(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.WebsiteAddInfor).ThenInclude(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (entry == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(Id, user.Id, Operation.EstablishWebsite))
            {
                return NotFound();
            }

            //获取用户的审核信息
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishWebsite);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            return _entryService.GetEditWebsitViewModel(entry);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditWebsiteAsync(EditEntryWebsiteViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _appHelper.IsEntryLockedAsync(model.Id, user.Id, Operation.EstablishWebsite))
            {
                return new Result { Error = "当前词条该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }

            //检查是否重复
            foreach (var item in model.Images)
            {
                if (model.Images.Count(s => s.Image == item.Image&&s.Type==item.Type) > 1)
                {
                    return new Result { Error = "相同类型的图片链接不能重复，重复的链接：" + item.Image, Successful = false };

                }
            }

            //查找词条
            var currentEntry = await _entryRepository.GetAll()
                .Include(s => s.WebsiteAddInfor).ThenInclude(s => s.Images)
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.WebsiteAddInfor).ThenInclude(s => s.Images)
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }

            //设置数据
            _entryService.SetDataFromEditWebsiteViewModel(newEntry, model);

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishWebsite) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishWebsite);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentEntry, user, examine.Key, Operation.EstablishWebsite, model.Note);



            return new Result { Successful = true };

        }

        [HttpPost]
        public async Task<ActionResult<Result>> EstablishEntryAsync(EstablishEntryViewModel model)
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
                //判断名称是否重复
                if (await _entryRepository.FirstOrDefaultAsync(s => s.Name == model.Main.Name) != null)
                {
                    return new Result { Error = "该词条的名称与其他词条重复", Successful = false };
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
                //检查是否重复
                foreach (var item in model.Audio.Audio)
                {
                    if (model.Audio.Audio.Count(s => s.Url == item.Url) > 1)
                    {
                        return new Result { Error = $"{item.Name} 与其他音频重复了，链接：{item.Url}", Successful = false };

                    }
                }
                //检查是否重复
                foreach (var item in model.Website.Images)
                {
                    if (model.Website.Images.Count(s => s.Image == item.Image && s.Type == item.Type) > 1)
                    {
                        return new Result { Error = "相同类型的图片链接不能重复，重复的链接：" + item.Image, Successful = false };

                    }
                }

                //判断关联是否存在
                var tags = new List<Tag>();

                var tagNames = new List<string>();
                tagNames.AddRange(model.Tags.Tags.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                tagNames = tagNames.Distinct().ToList();

                foreach (var item in tagNames)
                {
                    var infor = await _tagRepository.GetAll().Where(s => s.Name == item).FirstOrDefaultAsync();
                    if (infor == null)
                    {
                        return new Result { Successful = false, Error = "标签 " + item + " 不存在" };
                    }
                    else
                    {
                        tags.Add(infor);
                    }
                }

                //处理原始数据 删除空项目
                model.Relevances.Roles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.staffs.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.Groups.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.Games.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.articles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.news.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.others.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Relevances.videos.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));

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

                //视频
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
                var videos = await _videoRepository.GetAll().Where(s => videoIds.Contains(s.Id)).ToListAsync();
                //检查预约关联抽奖是否存在
                int lotteryId = 0;
                if (string.IsNullOrWhiteSpace(model.AddInfor.Booking.LotteryName) == false)
                {
                    var lottery = await _lotteryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden == false && s.Name == model.AddInfor.Booking.LotteryName).FirstOrDefaultAsync();
                    if (lottery == null)
                    {
                        return new Result { Successful = false, Error = "预约关联抽奖不存在" };
                    }
                    else
                    {
                        lotteryId = (int)lottery.Id;
                    }
                }

                var newEntry = new Entry();
                await _entryService.SetDataFromEditAddInforViewModelAsync(newEntry, model.AddInfor,lotteryId);
                _entryService.SetDataFromEditImagesViewModel(newEntry, model.Images);
                _entryService.SetDataFromEditMainPageViewModel(newEntry, model.MainPage);
                _entryService.SetDataFromEditMainViewModel(newEntry, model.Main);
                _entryService.SetDataFromEditRelevancesViewModel(newEntry, model.Relevances, entries, articles, videos);
                _entryService.SetDataFromEditTagsViewModel(newEntry, model.Tags, tags);
                _entryService.SetDataFromEditAudioViewModel(newEntry, model.Audio);
                _entryService.SetDataFromEditWebsiteViewModel(newEntry, model.Website);

                var entry = new Entry();
                //获取审核记录
                try
                {
                    entry = await _examineService.AddNewEntryExaminesAsync(newEntry, user, model.Note);
                }
                catch (Exception ex)
                {
                    return new Result { Successful = false, Error = ex.Message };

                }

                //创建词条成功
                return new Result { Successful = true, Error = entry.Id.ToString() };
            }
            catch (Exception)
            {
                return new Result { Error = "创建词条的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenEntryAsync(HiddenEntryModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            await _entryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));

            foreach(var item in model.Ids)
            {
                 _logger.LogInformation("管理员 - {name}({id}){operation}词条 - Id:{entryId}", user.UserName, user.Id, (model.IsHidden ? "隐藏" : "取消隐藏"), item);
            }
           

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> HideEntryOutlinkAsync(HiddenEntryModel model)
        {
            await _entryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHideOutlink, b => model.IsHidden));
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditEntryPriorityAsync(EditEntryPriorityViewModel model)
        {
            await _entryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));
            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> RevokeExamine(RevokeExamineModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //查找审核
            var examine = await _examineRepository.FirstOrDefaultAsync(s => s.EntryId == model.Id && s.ApplicationUserId == user.Id && s.Operation == model.ExamineType && s.IsPassed == null);
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

        /// <summary>
        /// 获取输入提示
        /// </summary>
        /// <returns></returns>
        [HttpGet("{type}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetAllEntriesAsync(EntryType type)
        {
            return await _entryRepository.GetAll().AsNoTracking().Where(s => s.Type == type && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Name).ToArrayAsync();
        }

        /// <summary>
        /// 获取所有词条名称ID对
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<NameIdPairModel>>> GetAllEntriesIdNameAsync()
        {
            return await _entryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => new NameIdPairModel { Name = s.Name, Id = s.Id }).ToArrayAsync();
        }

        /// <summary>
        /// 获取所有词条主图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetAllEntryMainImagesAsync()
        {
            var model = new List<KeyValuePair<string, string>>();
            model.AddRange(await _entryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false && string.IsNullOrWhiteSpace(s.MainPicture) == false)
               .Select(s => new KeyValuePair<string, string>("Entry_" + s.Id, s.MainPicture))
               .ToListAsync());
            model.AddRange(await _articleRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false && string.IsNullOrWhiteSpace(s.MainPicture) == false)
               .Select(s => new KeyValuePair<string, string>("Article_" + s.Id, s.MainPicture))
               .ToListAsync());
            model.AddRange(await _peripheryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false && string.IsNullOrWhiteSpace(s.MainPicture) == false)
               .Select(s => new KeyValuePair<string, string>("Periphery_" + s.Id, s.MainPicture))
               .ToListAsync());
            var result = "Key,Value\n";
            foreach (var item in model)
            {
                result += (item.Key + "," + item.Value + "\n");
            }
            return result;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EditEntryInforBindModel>> GetEntryEditInforBindModelAsync(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var model = new EditEntryInforBindModel
            {
                Id = id
            };
            //获取完善度
            model.PerfectionInfor = await _perfectionService.GetEntryPerfection(id);
            if (model.PerfectionInfor == null)
            {
                return NotFound("该词条不存在");
            }
            //获取完善度列表
            model.PerfectionChecks = await _perfectionService.GetEntryPerfectionCheckList(model.PerfectionInfor.Id);
            //获取编辑记录
            model.Examines = await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => s.EntryId == id && (s.IsPassed == true || (user != null && s.IsPassed == null && s.ApplicationUserId == user.Id))), true);
            model.Examines = model.Examines.OrderByDescending(s => s.ApplyTime).ToList();
            //获取编辑状态
            model.State = await _entryService.GetEntryEditState(user, id);
            //是否监视
            if (user != null)
            {
                model.IsInMonitor = await _editRecordService.CheckObjectIsInUserMonitor(user, id);

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
        public async Task<ActionResult<EntryContrastEditRecordViewModel>> GetContrastEditRecordViewsAsync(long contrastId, long currentId)
        {
            if (contrastId > currentId)
            {
                return BadRequest("对比的编辑必须先于当前的编辑");
            }

            var contrastExamine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == contrastId);
            var currentExamine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == currentId);

            if (contrastExamine == null || currentExamine == null || contrastExamine.EntryId == null || currentExamine.EntryId == null || contrastExamine.EntryId != currentExamine.EntryId)
            {
                return NotFound("编辑记录Id不正确");
            }

            var currentEntry = new Entry();
            var newEntry = new Entry();

            //获取审核记录
            var examines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.IsPassed == true && s.EntryId == currentExamine.EntryId).ToListAsync();

            foreach (var item in examines.Where(s => s.Id <= contrastId))
            {
                await _entryService.UpdateEntryDataAsync(currentEntry, item);
            }

            foreach (var item in examines.Where(s => s.Id <= currentId))
            {
                await _entryService.UpdateEntryDataAsync(newEntry, item);
            }

            var result = await _entryService.ConcompareAndGenerateModel(currentEntry, newEntry);

            var model = new EntryContrastEditRecordViewModel
            {
                ContrastId = contrastId,
                CurrentId = currentId,
                ContrastModel = result[0],
                CurrentModel = result[1],
            };

            return model;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<GameCGModel>>> GetGameCGsAsync()
        {
            var entryIds = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Pictures)
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.Pictures.Count > 3)
                .Select(s => s.Id).ToListAsync();

            entryIds = entryIds.Random().Take(20).ToList();

            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Pictures)
                .Where(s=> entryIds.Contains(s.Id))
                .Select(item => new GameCGModel
                {
                    Id = item.Id,
                    Name = item.DisplayName,
                    Pictures = item.Pictures.Select(s => new EditImageAloneModel
                    {
                        Note = s.Note,
                        Image = s.Url
                    }).ToList()
                })
                .ToListAsync();

            return entries;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<GameRoleModel>>> GetGameRolesAsync()
        {
            var entryIds = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.Type == EntryType.Game
                        && s.EntryRelationFromEntryNavigation.Count(s => s.ToEntryNavigation.Type == EntryType.Role && string.IsNullOrWhiteSpace(s.ToEntryNavigation.MainPicture) == false) > 4)
                .Select(s => s.Id).ToListAsync();

            entryIds = entryIds.Random().Take(40).ToList();

            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                 .Where(s => entryIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.DisplayName,
                    s.MainPicture,
                    Roles = s.EntryRelationFromEntryNavigation.Where(s => s.ToEntryNavigation.Type == EntryType.Role).Select(s => s.ToEntryNavigation)
                })
                .ToListAsync();

            var model = new List<GameRoleModel>();

            foreach (var item in entries)
            {
                var temp = new GameRoleModel
                {
                    Name = item.DisplayName,
                    Id = item.Id,
                    Image = _appHelper.GetImagePath(item.MainPicture, "app.png")
                };
                foreach (var infor in item.Roles)
                {
                    var infor1 = _appHelper.GetEntryInforTipViewModel(infor);
                    temp.Roles.Add(infor1);
                }

                model.Add(temp);
            }

            return model;
        }

        [AllowAnonymous]
        [HttpGet("{name}")]
        public async Task<ActionResult<int>> GetId(string name)
        {
            var name_ = ToolHelper.Base64DecodeName(name);
            return await _entryRepository.GetAll().AsNoTracking().Where(s => s.Name == name_).Select(s => s.Id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 获取每月发布的游戏
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<EntryInforTipViewModel>>> GetPublishGamesByTime([FromQuery]int year, [FromQuery]int month)
        {
            DateTime time = DateTime.Now.ToCstTime();

            if(month==0)
            {
                month = time.Month;
            }
            if (year == 0)
            {
                year = time.Year;
            }
            var games = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false)
                .Where(s => s.PubulishTime != null && s.PubulishTime.Value.Year == year && s.PubulishTime.Value.Month == month)
                .Where(s => s.Releases.Any(s => s.Time != null && s.Type == GameReleaseType.Official))
                .Where(s => string.IsNullOrWhiteSpace(s.Releases.OrderBy(s => s.Time).First(s => s.Time != null && s.Type == GameReleaseType.Official).TimeNote) || s.PubulishTime < time)
                .ToListAsync();

            var model = new List<EntryInforTipViewModel>();
            foreach(var item in games)
            {
                model.Add( _appHelper.GetEntryInforTipViewModel(item));
            }

            return model;
        }

        /// <summary>
        /// 获取每月角色生日
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<RoleBrithdayViewModel>>> GetRoleBirthdaysByTime([FromQuery] int month,[FromQuery] int day)
        {
            return await _entryService.GetBirthdayRoles(month,day);
        }

        /// <summary>
        /// 获取 发布游戏时间轴
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="afterTime"></param>
        /// <param name="beforeTime"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<PublishGamesTimelineModel>>> GetPublishGamesTimeline([FromQuery] int groupId, [FromQuery] long afterTime, [FromQuery] long beforeTime)
        {
            DateTime after = DateTime.MinValue;
            DateTime before = DateTime.MaxValue;

            if (afterTime!=0)
            {
                after = afterTime.ToString().TransTime();       
            }
            if(beforeTime!=0)
            {
                before = beforeTime.ToString().TransTime();
            }

            DateTime time = DateTime.Now.ToCstTime();
            var games = await _entryRepository.GetAll().AsNoTracking()
                .Include(s=>s.Releases)
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false)
                .Where(s=>groupId==0||s.EntryStaffFromEntryNavigation.Any(s=>s.PositionGeneral== PositionGeneralType.ProductionGroup&&s.ToEntry==groupId))
                .Where(s => s.PubulishTime != null && s.PubulishTime.Value.Date> after && s.PubulishTime.Value.Date< before)
                .Where(s => s.Releases.Any(s => s.Time != null && s.Type == GameReleaseType.Official))
                .Where(s => string.IsNullOrWhiteSpace(s.Releases.OrderBy(s => s.Time).First(s => s.Time != null && s.Type == GameReleaseType.Official).TimeNote) || s.PubulishTime < time)
                .ToListAsync();

            var model = new List<PublishGamesTimelineModel>();
            foreach (var item in games)
            {
                var entry = new PublishGamesTimelineModel();
                entry.SynchronizationProperties(_appHelper.GetEntryInforTipViewModel(item));

                entry.PublishTimeNote= item.Releases.OrderBy(s=>s.Time).FirstOrDefault(s =>s.Type== GameReleaseType.Official)?.TimeNote;

                model.Add(entry);
            }

            return model;
        }

        /// <summary>
        /// 列出所有词条基础信息类型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<EntryInformationTypeOverviewModel>> ListEntryInformationTypes(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<EntryInformationType, long>(_entryInformationTypeRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || ( s.Name.Contains(model.SearchText) || s.Description.Contains(model.SearchText)));

            return new QueryResultModel<EntryInformationTypeOverviewModel>
            {
                Items = await items.Select(s => new EntryInformationTypeOverviewModel
                {
                    Icon= s.Icon,
                    IsHidden= s.IsHidden,
                    Name= s.Name,
                    Types = s.Types.ToList(),
                    Description = s.Description,
                    Id = s.Id,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        /// <summary>
        /// 获取词条基础信息类型编辑模型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<EntryInformationTypeEditModel>> EditEntryInformationType(long id)
        {
            var item = await _entryInformationTypeRepository.GetAll()
              .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("找不到目标词条基础信息类型");
            }

            var model = new EntryInformationTypeEditModel
            {
                Icon = item.Icon,
                IsHidden = item.IsHidden,
                Name = item.Name,
                Types = item.Types.ToList(),
                Description = item.Description,
                Id = item.Id,
            };

            return model;
        }

        /// <summary>
        /// 编辑词条基础信息类型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Result> EditEntryInformationType(EntryInformationTypeEditModel model)
        {

            EntryInformationType item = null;
            if (model.Id == 0)
            {
                item = await _entryInformationTypeRepository.InsertAsync(new EntryInformationType
                {
                    Icon = item.Icon,
                    IsHidden = item.IsHidden,
                    Name = item.Name,
                    Types = item.Types,
                    Description = item.Description,
                });
                model.Id = item.Id;
                _entryInformationTypeRepository.Clear();
            }

            item = await _entryInformationTypeRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == model.Id);


            if (item == null)
            {
                return new Result { Successful = false, Error = "目标不存在" };
            }



            //基本信息

            item.Name = model.Name;
            item.Icon = model.Icon;
            item.Description = model.Description;
            item.IsHidden = model.IsHidden;
            item.Types = model.Types.ToArray();

            //保存
            await _entryInformationTypeRepository.UpdateAsync(item);


            return new Result { Successful = true };
        }

        /// <summary>
        /// 获取对应类型词条的基础信息类型列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<EditInformationModel>> GetEditInformationModelList(int type)
        {
            return (await _entryInformationTypeRepository.GetAll()
                             .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false)
                             .ToListAsync())
                             .Where(s => s.Types.Contains((EntryType)type))
                             .Select(s => new EditInformationModel
                             {
                                 Description = s.Description,
                                 Icon = s.Icon,
                                 Name = s.Name,
                             }).ToList();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<EntryOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Entry, int>(_entryRepository.GetAll().AsSingleQuery().Where(s=>string.IsNullOrWhiteSpace(s.Name)==false), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<EntryOverviewModel>
            {
                Items = await items.Select(s => new EntryOverviewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsHidden = s.IsHidden,
                    CanComment = s.CanComment ?? true,
                    Priority = s.Priority,
                    Type = s.Type,
                    IsHideOutlink = s.IsHideOutlink,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
