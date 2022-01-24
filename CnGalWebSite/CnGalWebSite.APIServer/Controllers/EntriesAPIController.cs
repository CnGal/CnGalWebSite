using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Search;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TencentCloud.Cme.V20191029.Models;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/entries/[action]")]
    public class EntriesAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IAppHelper _appHelper;
        private readonly IEntryService _entryService;
        private readonly IArticleService _articleService;
        private readonly IExamineService _examineService;
        private readonly IPerfectionService _perfectionService;

        public EntriesAPIController(UserManager<ApplicationUser> userManager, IRepository<Article, long> articleRepository, IRepository<Periphery, long> peripheryRepository,
        IPerfectionService perfectionService, IRepository<Examine, long> examineRepository, IArticleService articleService,
        IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Tag, int> tagRepository, IEntryService entryService, IExamineService examineService)
        {
            _userManager = userManager;
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
        }

        /// <summary>
        /// 通过Id获取词条的真实数据 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Entry>> GetEntryDataAsync(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var entry = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Outlinks)
                .Include(s => s.EntryRelationFromEntryNavigation)
                .Include(s => s.Articles)
                .Include(s => s.Examines)
                .Include(s => s.Information).ThenInclude(s => s.Additional)
                .Include(s => s.Tags)
                .Include(s => s.Pictures).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id && x.IsHidden != true);
            if (entry == null)
            {
                return NotFound();
            }
            else
            {
                //判断当前是否隐藏
                if (entry.IsHidden == true)
                {
                    if (user == null || await _userManager.IsInRoleAsync(user, "Admin") != true)
                    {

                        return NotFound();
                    }
                    entry.IsHidden = true;
                }
                else
                {
                    entry.IsHidden = false;
                }

                //需要清除环回引用
                foreach (var item in entry.Examines)
                {
                    item.Entry = null;
                }
                //需要清除环回引用
                foreach (var item in entry.Tags)
                {
                    item.Entries = null;
                }
                //需要清除环回引用
                foreach (var item in entry.Articles)
                {
                    item.Entries = null;
                }
                //需要清除环回引用
                foreach (var item in entry.EntryRelationFromEntryNavigation)
                {
                    item.FromEntryNavigation = null;
                    item.ToEntryNavigation = null;
                }
                return entry;
            }

        }

        /// <summary>
        /// 通过Id获取为页面显示优化后的词条信息
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        [HttpGet("{_id}")]
        [AllowAnonymous]
        // [Authorize]
        public async Task<ActionResult<EntryIndexViewModel>> GetEntryViewAsync(string _id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取词条 
            Entry entry = null;
            try
            {
                var id = -1;
                id = int.Parse(_id);
                entry = await _entryRepository.GetAll().Include(s => s.Disambig)
                    .Include(s => s.Outlinks)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.Articles).ThenInclude(s => s.CreateUser)
                    .Include(s => s.Articles).ThenInclude(s => s.Entries)
                    .Include(s => s.Information).ThenInclude(s => s.Additional).Include(s => s.Tags).Include(s => s.Pictures)
                    .AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                entry = await _entryRepository.GetAll().Include(s => s.Disambig)
                    .Include(s => s.Outlinks)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.Articles).ThenInclude(s => s.CreateUser)
                    .Include(s => s.Articles).ThenInclude(s => s.Entries)
                    .Include(s => s.Information).ThenInclude(s => s.Additional).Include(s => s.Tags).Include(s => s.Pictures)
                    .AsSplitQuery().FirstOrDefaultAsync(x => x.Name == ToolHelper.Base64DecodeName(_id));
            }

            if (entry == null)
            {
                return NotFound();
            }
            //判断当前是否隐藏
            if (entry.IsHidden == true)
            {
                if (user == null || await _userManager.IsInRoleAsync(user, "Editor") != true)
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
                               || s.Operation == Operation.EstablishRelevances || s.Operation == Operation.EstablishTags))
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
            }

            //建立视图模型
            var model = await _entryService.GetEntryIndexViewModelAsync(entry);
            model.Examines = await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => s.EntryId == entry.Id && s.IsPassed == true).OrderByDescending(s=>s.Id), true);

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
                        model.MainState = EditState.locked;
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
                        model.InforState = EditState.locked;
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
                        model.MainPageState = EditState.locked;
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
                        model.ImagesState = EditState.locked;
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
                        model.RelevancesState = EditState.locked;
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
                        model.TagState = EditState.locked;
                    }
                    else
                    {
                        model.TagState = EditState.Normal;
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


        /// <summary>
        /// 获取随机词条列表 
        /// </summary>
        /// <returns></returns>
        [HttpGet("{type}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetRandomEntryListViewAsync(EntryType type)
        {
            var model = new List<EntryHomeAloneViewModel>();
            var length = await _entryRepository.GetAll().AsNoTracking().Where(s => s.Type == type && s.IsHidden != true && s.Name != null && s.Name != "").CountAsync();
            var length_1 = (type == EntryType.Game || type == EntryType.ProductionGroup) ? 16 : 24;
            List<Entry> groups;
            if (length > length_1)
            {
                var random = new Random();
                var temp = random.Next(0, length - length_1);

                groups = await _entryRepository.GetAll().AsNoTracking().Where(s => s.Type == type && s.IsHidden != true && s.Name != null && s.Name != "").Skip(temp).Take(length_1).ToListAsync();
            }
            else
            {
                groups = await _entryRepository.GetAll().AsNoTracking().Where(s => s.Type == type && s.IsHidden != true && s.Name != null && s.Name != "").Take(length_1).ToListAsync();
            }
            foreach (var item in groups)
            {
                model.Add(new EntryHomeAloneViewModel
                {
                    Id = item.Id,
                    Image = ((type == EntryType.Game || type == EntryType.ProductionGroup) ? _appHelper.GetImagePath(item.MainPicture, "app.png") : _appHelper.GetImagePath(item.Thumbnail, "user.png")),
                    DisPlayName = item.DisplayName ?? item.Name,
                    CommentCount = item.CommentCount,
                    ReadCount = item.ReaderCount,
                    // DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                });
            }
            return model;
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
            var model = new EditMainViewModel();
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishMain);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            model.MainPicturePath = _appHelper.GetImagePath(entry.MainPicture, "app.png");
            model.ThumbnailPath = _appHelper.GetImagePath(entry.Thumbnail, "app.png");
            model.BackgroundPicturePath = _appHelper.GetImagePath(entry.BackgroundPicture, "app.png");
            model.Thumbnail = entry.Thumbnail;
            model.MainPicture = entry.MainPicture;
            model.BackgroundPicture = entry.BackgroundPicture;

            model.Name = entry.Name;
            model.BriefIntroduction = entry.BriefIntroduction;
            model.Type = entry.Type;
            model.DisplayName = entry.DisplayName;
            model.AnotherName = entry.AnotherName;
            model.SmallBackgroundPicture = entry.SmallBackgroundPicture;
            model.SmallBackgroundPicturePath = _appHelper.GetImagePath(entry.SmallBackgroundPicture, "background.png");

            model.Id = entry.Id;

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

            newEntry.Name = model.Name;
            newEntry.BriefIntroduction = model.BriefIntroduction;
            newEntry.MainPicture = model.MainPicture;
            newEntry.Thumbnail = model.Thumbnail;
            newEntry.BackgroundPicture = model.BackgroundPicture;
            newEntry.Type = model.Type;
            newEntry.DisplayName = model.DisplayName;
            newEntry.SmallBackgroundPicture = model.SmallBackgroundPicture;
            newEntry.AnotherName = model.AnotherName;

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if(examines.Any(s => s.Value == Operation.EstablishMain)==false)
            {
                return new Result { Successful = true };
            }
            var examine=examines.FirstOrDefault(s => s.Value == Operation.EstablishMain);
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, examine.Key);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishMainAsync(currentEntry, examine.Key as ExamineMain);
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, true, resulte, Operation.EstablishMain, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, currentEntry.Id, Operation.EstablishMain);
            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, false, resulte, Operation.EstablishMain, model.Note);
            }


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditAddInforViewModel>> EditAddInforAsync(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Information).ThenInclude(s => s.Additional).FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
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
            var model = new EditAddInforViewModel
            {
                Type = entry.Type,
                IsRealSubmit = "false",
                Id = Id,
                Name = entry.Name,
                SocialPlatforms = new List<SocialPlatform>()
            };

            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishAddInfor);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            //先加载词条当前数据
            var information = new List<BasicEntryInformation>();
            foreach (var item in entry.Information)
            {
                information.Add(item);
            }


            //根据类别进行序列化
            switch (entry.Type)
            {
                case EntryType.Game:
                    model.Staffs = new List<StaffModel>();
                    model.GamePlatforms = new List<GamePlatformModel>
                    {
                        new GamePlatformModel { GamePlatformType = GamePlatformType.Android, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.Windows, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.DOS, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.IOS, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.Linux, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.Mac, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.NS, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.PS, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.HarmonyOS, IsSelected = false }
                    };
                    //遍历基本信息
                    foreach (var item in information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "发行时间":
                                    try
                                    {
                                        model.IssueTime = DateTime.ParseExact(item.DisplayValue, "yyyy年M月d日", null);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            model.IssueTime = DateTime.ParseExact(item.DisplayValue, "yyyy/M/d", null);
                                        }
                                        catch
                                        {
                                            model.IssueTime = null;
                                        }
                                    }

                                    break;
                                case "发行时间备注":
                                    model.IssueTimeString = item.DisplayValue;
                                    break;
                                case "原作":
                                    model.Original = item.DisplayValue;
                                    break;
                                case "制作组":
                                    model.ProductionGroup = item.DisplayValue;
                                    break;
                                case "游戏平台":

                                    var sArray = Regex.Split(item.DisplayValue, "、", RegexOptions.IgnoreCase);
                                    foreach (var str in sArray)
                                    {
                                        switch (str)
                                        {
                                            case "Windows":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.Windows)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "Linux":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.Linux)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "Mac":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.Mac)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "IOS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.IOS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "Android":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.Android)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "PS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.PS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "NS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.NS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "DOS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.DOS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "HarmonyOS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.HarmonyOS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case "引擎":
                                    model.Engine = item.DisplayValue;
                                    break;
                                case "发行商":
                                    model.Publisher = item.DisplayValue;
                                    break;
                                case "发行方式":
                                    model.IssueMethod = item.DisplayValue;
                                    break;
                                case "官网":
                                    model.OfficialWebsite = item.DisplayValue;
                                    break;
                                case "Steam平台Id":
                                    model.SteamId = item.DisplayValue;
                                    break;
                                case "QQ群":
                                    model.QQgroupGame = item.DisplayValue;
                                    break;
                            }
                        }
                        else if (item.Modifier == "STAFF")
                        {
                            var staffModel = new StaffModel
                            {
                                Id = item.Id
                            };
                            foreach (var infor in item.Additional)
                            {
                                switch (infor.DisplayName)
                                {
                                    case "职位（官方称呼）":
                                        staffModel.PositionOfficial = infor.DisplayValue;
                                        break;
                                    case "昵称（官方称呼）":
                                        staffModel.NicknameOfficial = infor.DisplayValue;
                                        break;
                                    case "职位（通用）":
                                        staffModel.PositionGeneral = (PositionGeneralType)Enum.Parse(typeof(PositionGeneralType), infor.DisplayValue);
                                        break;
                                    case "角色":
                                        staffModel.Role = infor.DisplayValue;
                                        break;
                                    case "隶属组织":
                                        staffModel.SubordinateOrganization = infor.DisplayValue;
                                        break;
                                    case "子项目":
                                        staffModel.Subcategory = infor.DisplayValue;
                                        break;
                                }
                            }
                            model.Staffs.Add(staffModel);

                        }
                        else if (item.Modifier == "相关网站")
                        {
                            var socialPlatform = new SocialPlatform
                            {
                                Name = item.DisplayName,
                                Link = item.DisplayValue
                            };

                            model.SocialPlatforms.Add(socialPlatform);

                        }
                    }
                    break;
                case EntryType.ProductionGroup:
                    //遍历基本信息
                    foreach (var item in information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "QQ群":
                                    model.QQgroupGroup = item.DisplayValue;
                                    break;
                            }
                        }
                        else if (item.Modifier == "相关网站")
                        {
                            if (item.DisplayValue.Contains("weibo.com"))
                            {
                                model.WeiboId = item.DisplayValue.Replace("https://weibo.com/u/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("bilibili.com"))
                            {
                                model.BilibiliId = item.DisplayValue.Replace("https://space.bilibili.com/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("acfun.cn"))
                            {
                                model.AcFunId = item.DisplayValue.Replace("https://www.acfun.cn/u/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("zhihu.com"))
                            {
                                model.ZhihuId = item.DisplayValue.Replace("https://www.zhihu.com/people/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("afdian.net"))
                            {
                                model.AfdianId = item.DisplayValue.Replace("https://afdian.net/@", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("pixiv.net"))
                            {
                                model.PixivId = item.DisplayValue.Replace("https://www.pixiv.net/users/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("twitter.com"))
                            {
                                model.TwitterId = item.DisplayValue.Replace("https://twitter.com/", "").Replace("https://www.twitter.com/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("youtube.com"))
                            {
                                model.YouTubeId = item.DisplayValue.Replace("https://www.youtube.com/channel/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("facebook.com"))
                            {
                                model.FacebookId = item.DisplayValue.Replace("https://www.facebook.com/", "").Split('/').FirstOrDefault();

                            }
                            else
                            {
                                var socialPlatform = new SocialPlatform
                                {
                                    Name = item.DisplayName,
                                    Link = item.DisplayValue
                                };

                                model.SocialPlatforms.Add(socialPlatform);
                            }

                        }

                    }
                    break;
                case EntryType.Role:
                    //遍历基本信息
                    foreach (var item in information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "声优":
                                    model.CV = item.DisplayValue;
                                    break;
                                case "性别":
                                    model.Gender = (GenderType)Enum.Parse(typeof(GenderType), item.DisplayValue);
                                    break;
                                case "身材数据":
                                    model.FigureData = item.DisplayValue;
                                    break;
                                case "身材(主观)":
                                    model.FigureSubjective = item.DisplayValue;
                                    break;
                                case "生日":
                                    try
                                    {
                                        model.Birthday = DateTime.ParseExact(item.DisplayValue, "M月d日", null);
                                    }
                                    catch
                                    {

                                        model.Birthday = null;

                                    }
                                    break;
                                case "发色":
                                    model.Haircolor = item.DisplayValue;
                                    break;
                                case "瞳色":
                                    model.Pupilcolor = item.DisplayValue;
                                    break;
                                case "服饰":
                                    model.ClothesAccessories = item.DisplayValue;
                                    break;
                                case "性格":
                                    model.Character = item.DisplayValue;
                                    break;
                                case "角色身份":
                                    model.RoleIdentity = item.DisplayValue;
                                    break;
                                case "血型":
                                    model.BloodType = item.DisplayValue;
                                    break;
                                case "身高":
                                    model.RoleHeight = item.DisplayValue;
                                    break;
                                case "兴趣":
                                    model.RoleTaste = item.DisplayValue;
                                    break;
                                case "年龄":
                                    model.RoleAge = item.DisplayValue;
                                    break;
                            }
                        }
                        else if (item.Modifier == "相关网站")
                        {
                            var socialPlatform = new SocialPlatform
                            {
                                Name = item.DisplayName,
                                Link = item.DisplayValue
                            };

                            model.SocialPlatforms.Add(socialPlatform);

                        }

                    }
                    break;
                case EntryType.Staff:
                    //遍历基本信息
                    foreach (var item in information)
                    {
                        if (item.Modifier == "基本信息")
                        {

                        }
                        else if (item.Modifier == "相关网站")
                        {
                            if (item.DisplayValue.Contains("weibo.com"))
                            {
                                model.WeiboId = item.DisplayValue.Replace("https://weibo.com/u/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("bilibili.com"))
                            {
                                model.BilibiliId = item.DisplayValue.Replace("https://space.bilibili.com/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("acfun.cn"))
                            {
                                model.AcFunId = item.DisplayValue.Replace("https://www.acfun.cn/u/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("zhihu.com"))
                            {
                                model.ZhihuId = item.DisplayValue.Replace("https://www.zhihu.com/people/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("afdian.net"))
                            {
                                model.AfdianId = item.DisplayValue.Replace("https://afdian.net/@", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("pixiv.net"))
                            {
                                model.PixivId = item.DisplayValue.Replace("https://www.pixiv.net/users/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("twitter.com"))
                            {
                                model.TwitterId = item.DisplayValue.Replace("https://twitter.com/", "").Replace("https://www.twitter.com/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("youtube.com"))
                            {
                                model.YouTubeId = item.DisplayValue.Replace("https://www.youtube.com/channel/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("facebook.com"))
                            {
                                model.FacebookId = item.DisplayValue.Replace("https://www.facebook.com/", "").Split('/').FirstOrDefault();

                            }
                            else
                            {
                                var socialPlatform = new SocialPlatform
                                {
                                    Name = item.DisplayName,
                                    Link = item.DisplayValue
                                };

                                model.SocialPlatforms.Add(socialPlatform);
                            }

                        }
                    }


                    break;
            }

            return model;
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
            var currentEntry = await _entryRepository.GetAll().Include(s => s.Information).ThenInclude(s => s.Additional).FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Information).ThenInclude(s => s.Additional).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }
            newEntry.Information.Clear();
            //根据类别进行序列化操作
            switch (model.Type)
            {
                case EntryType.Game:
                    //遍历一遍当前视图中staffs 
                    foreach (var item in model.Staffs)
                    {

                        var staffs = new List<BasicEntryInformationAdditional>
                        {
                            new BasicEntryInformationAdditional { DisplayName = "职位（官方称呼）", DisplayValue = item.PositionOfficial },
                            new BasicEntryInformationAdditional { DisplayName = "昵称（官方称呼）", DisplayValue = item.NicknameOfficial },
                            new BasicEntryInformationAdditional { DisplayName = "职位（通用）", DisplayValue = item.PositionGeneral.ToString() },
                            new BasicEntryInformationAdditional { DisplayName = "角色", DisplayValue = item.Role },
                            new BasicEntryInformationAdditional { DisplayName = "隶属组织", DisplayValue = item.SubordinateOrganization },
                            new BasicEntryInformationAdditional { DisplayName = "子项目", DisplayValue = item.Subcategory }
                        };
                        newEntry.Information.Add(new BasicEntryInformation
                        {
                            Modifier = "STAFF",
                            DisplayName = item.Subcategory + item.PositionOfficial,
                            DisplayValue = item.NicknameOfficial,
                            Additional = staffs
                        });
                    }
                    //序列化游戏平台
                    string gamePlatforms = null;
                    var isFirst = true;
                    foreach (var item in model.GamePlatforms)
                    {
                        if (item.IsSelected == true)
                        {
                            if (isFirst == true)
                            {
                                isFirst = false;
                            }
                            else
                            {
                                gamePlatforms += "、";
                            }
                            gamePlatforms += item.GamePlatformType.ToString();
                        }
                    }
                    //添加基本信息
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间", DisplayValue = model.IssueTime?.ToString("yyyy年M月d日") });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间备注", DisplayValue = model.IssueTimeString });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "原作", DisplayValue = model.Original });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "制作组", DisplayValue = model.ProductionGroup });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "游戏平台", DisplayValue = gamePlatforms });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "引擎", DisplayValue = model.Engine });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行商", DisplayValue = model.Publisher });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行方式", DisplayValue = model.IssueMethod });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "官网", DisplayValue = model.OfficialWebsite });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "Steam平台Id", DisplayValue = model.SteamId });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGame });
                    break;
                case EntryType.ProductionGroup:
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGroup });
                    break;
                case EntryType.Role:
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "声优", DisplayValue = model.CV });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性别", DisplayValue = model.Gender.ToString() });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材数据", DisplayValue = model.FigureData });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材(主观)", DisplayValue = model.FigureSubjective });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "生日", DisplayValue = model.Birthday?.ToString("M月d日") });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发色", DisplayValue = model.Haircolor });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "瞳色", DisplayValue = model.Pupilcolor });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性格", DisplayValue = model.Character });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "角色身份", DisplayValue = model.RoleIdentity });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "血型", DisplayValue = model.BloodType });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身高", DisplayValue = model.RoleHeight });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "兴趣", DisplayValue = model.RoleTaste });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "年龄", DisplayValue = model.RoleAge });


                    break;
                case EntryType.Staff:
                    break;
            }
            //序列化相关网站

            //加载在基本信息中的网站
            if (string.IsNullOrWhiteSpace(model.WeiboId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "微博",
                    Link = "https://weibo.com/u/" + model.WeiboId
                });
            }
            if (string.IsNullOrWhiteSpace(model.BilibiliId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "Bilibili",
                    Link = "https://space.bilibili.com/" + model.BilibiliId
                });
            }
            if (string.IsNullOrWhiteSpace(model.AcFunId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "AcFun",
                    Link = "https://www.acfun.cn/u/" + model.AcFunId
                });
            }
            if (string.IsNullOrWhiteSpace(model.ZhihuId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "知乎",
                    Link = "https://www.zhihu.com/people/" + model.ZhihuId
                });
            }
            if (string.IsNullOrWhiteSpace(model.AfdianId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "爱发电",
                    Link = "https://afdian.net/@" + model.AfdianId
                });
            }
            if (string.IsNullOrWhiteSpace(model.PixivId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "Pixiv",
                    Link = "https://www.pixiv.net/users/" + model.PixivId
                });
            }
            if (string.IsNullOrWhiteSpace(model.TwitterId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "Twitter",
                    Link = "https://twitter.com/" + model.TwitterId
                });
            }
            if (string.IsNullOrWhiteSpace(model.YouTubeId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "YouTube",
                    Link = "https://www.youtube.com/channel/" + model.YouTubeId
                });
            }
            if (string.IsNullOrWhiteSpace(model.FacebookId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "Facebook",
                    Link = "https://www.facebook.com/" + model.FacebookId
                });
            }


            //遍历一遍当前视图中 相关网站
            foreach (var item in model.SocialPlatforms)
            {
                newEntry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "相关网站",
                    DisplayName = item.Name,
                    DisplayValue = item.Link
                });
            }

            var tempList = newEntry.Information.ToList();
            tempList.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayValue));
            newEntry.Information = tempList;

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishAddInfor) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishAddInfor);
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, examine.Key);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishAddInforAsync(currentEntry, examine.Key as EntryAddInfor);
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, true, resulte, Operation.EstablishAddInfor, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, currentEntry.Id, Operation.EstablishAddInfor);
            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, false, resulte, Operation.EstablishAddInfor, model.Note);
            }

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
            //根据类别生成首个视图模型
            var model = new EditImagesViewModel
            {
                Name = entry.Name,
                Id = Id,
            };
            //获取用户的审核信息
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishImages);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }
            //处理图片
            var Images = new List<EditImageAloneModel>();
            foreach (var item in entry.Pictures)
            {
                Images.Add(new EditImageAloneModel
                {
                    Url = _appHelper.GetImagePath(item.Url, ""),
                    Modifier = item.Modifier,
                    Note = item.Note
                });
            }

            model.Images = Images;
            return model;
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
                if (item.Url.Contains("image.cngal.org") == false && item.Url.Contains("pic.cngal.top") == false)
                {
                    return new Result { Successful = false, Error = "相册中不能添加外部图片：" + item.Url };
                }
            }
            //查找词条
            var currentEntry = await _entryRepository.GetAll().Include(s => s.Pictures).FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Pictures).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (currentEntry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }


            //再遍历视图模型中的图片 对应修改

            foreach (var item in model.Images)
            {

                newEntry.Pictures.Add(new EntryPicture
                {
                    Url = item.Url,
                    Modifier = item.Modifier,
                    Note = item.Note
                });

            }

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishImages) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishImages);
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, examine.Key);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishImagesAsync(currentEntry, examine.Key as EntryImages);
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, true, resulte, Operation.EstablishImages, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, currentEntry.Id, Operation.EstablishImages);
            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, false, resulte, Operation.EstablishImages, model.Note);
            }
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
            var model = new EditRelevancesViewModel
            {
                Id = entry.Id,
                Name = entry.Name,
                Type = entry.Type
            };
            //获取用户的审核信息
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishRelevances);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);

            }
            //处理附加信息
            var roles = new List<RelevancesModel>();
            var staffs = new List<RelevancesModel>();
            var articles = new List<RelevancesModel>();
            var groups = new List<RelevancesModel>();
            var games = new List<RelevancesModel>();
            var news = new List<RelevancesModel>();
            var others = new List<RelevancesModel>();
            foreach (var nav in entry.EntryRelationFromEntryNavigation)
            {
                var item = nav.ToEntryNavigation;
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
            foreach (var item in entry.Articles)
            {
                switch (item.Type)
                {
                    case ArticleType.News:
                        news.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    default:
                        articles.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                }
            }
            foreach (var item in entry.Outlinks)
            {
                others.Add(new RelevancesModel
                {
                    DisplayName = item.Name,
                    DisPlayValue = item.BriefIntroduction,
                    Link = item.Link
                });
            }

            model.Roles = roles;
            model.staffs = staffs;
            model.articles = articles;
            model.Groups = groups;
            model.Games = games;
            model.others = others;
            model.news = news;

            return model;
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
              .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
              .FirstOrDefaultAsync(x => x.Id == model.Id);
            var newEntry = await _entryRepository.GetAll().AsNoTracking()
              .Include(s => s.Outlinks)
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
            model.others.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));


            //预处理 建立词条关联信息
            //判断关联是否存在
            var entryIds = new List<int>();
            var entryNames = new List<string>();

            var articleIds = new List<long>();
            var articleNames = new List<string>();

            entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            //建立文章关联信息
            articleNames.AddRange(model.articles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            articleNames.AddRange(model.news.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            try
            {
                entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }
            //获取词条文章
            var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();
            var articles = await _articleRepository.GetAll().Where(s => articleIds.Contains(s.Id)).ToListAsync();

            newEntry.Outlinks.Clear();
            newEntry.Articles = articles;
            newEntry.EntryRelationFromEntryNavigation = entries.Select(s => new EntryRelation
            {
                ToEntry = s.Id,
                ToEntryNavigation = s
            }).ToList();

            foreach (var item in model.others)
            {
                newEntry.Outlinks.Add(new Outlink
                {
                    Name = item.DisplayName,
                    BriefIntroduction = item.DisPlayValue,
                    Link = item.Link,
                });
            }

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishRelevances) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishRelevances);
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, examine.Key);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishRelevancesAsync(currentEntry, examine.Key as EntryRelevances);
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, true, resulte, Operation.EstablishRelevances, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, currentEntry.Id, Operation.EstablishRelevances);
            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, false, resulte, Operation.EstablishRelevances, model.Note);
            }

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
            var model = new EditMainPageViewModel();
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishMainPage);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }

            model.Context = entry.MainPage;
            model.Id = entry.Id;
            model.Type = entry.Type;
            model.Name = entry.Name;

            return model;
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

            newEntry.MainPage = model.Context;

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishMainPage) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishMainPage);
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, examine.Key);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishMainPageAsync(currentEntry, examine.Key as string);
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, true, resulte, Operation.EstablishMainPage, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, currentEntry.Id, Operation.EstablishMainPage);
            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, false, resulte, Operation.EstablishMainPage, model.Note);
            }

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
                if (await _entryRepository.FirstOrDefaultAsync(s => s.Name == model.Name) != null)
                {
                    return new Result { Error = "该词条的名称与其他词条重复", Successful = false };
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

                //处理原始数据 删除空项目
                model.Roles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.ReStaffs.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Groups.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Games.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.articles.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.News.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));
                model.Others.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayName));


                //预处理 建立词条关联信息
                //判断关联是否存在
                var entryIds = new List<int>();
                var entryNames = new List<string>();

                var articleIds = new List<long>();
                var articleNames = new List<string>();

                entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.ReStaffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

                //建立文章关联信息
                articleNames.AddRange(model.articles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                articleNames.AddRange(model.News.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

                try
                {
                    entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                    articleIds = await _articleService.GetArticleIdsFromNames(articleNames);
                }
                catch (Exception ex)
                {
                    return new Result { Successful = false, Error = ex.Message };
                }

                var newEntry = new Entry();

                //第一步 建立词条主要信息

                newEntry.Name = model.Name;
                newEntry.BriefIntroduction = model.BriefIntroduction;
                newEntry.MainPicture = model.MainPicture;
                newEntry.Thumbnail = model.Thumbnail;
                newEntry.BackgroundPicture = model.BackgroundPicture;
                newEntry.Type = model.Type;
                newEntry.DisplayName = model.DisplayName;
                newEntry.SmallBackgroundPicture = model.SmallBackgroundPicture;
                newEntry.AnotherName = model.AnotherName;

                //第二步 建立词条附加信息
                //根据类别进行序列化操作
                switch (model.Type)
                {
                    case EntryType.Game:
                        //遍历一遍当前视图中staffs 
                        foreach (var item in model.InforStaffs)
                        {

                            var staffs = new List<BasicEntryInformationAdditional>
                        {
                            new BasicEntryInformationAdditional { DisplayName = "职位（官方称呼）", DisplayValue = item.PositionOfficial },
                            new BasicEntryInformationAdditional { DisplayName = "昵称（官方称呼）", DisplayValue = item.NicknameOfficial },
                            new BasicEntryInformationAdditional { DisplayName = "职位（通用）", DisplayValue = item.PositionGeneral.ToString() },
                            new BasicEntryInformationAdditional { DisplayName = "角色", DisplayValue = item.Role },
                            new BasicEntryInformationAdditional { DisplayName = "隶属组织", DisplayValue = item.SubordinateOrganization },
                            new BasicEntryInformationAdditional { DisplayName = "子项目", DisplayValue = item.Subcategory }
                        };
                            newEntry.Information.Add(new BasicEntryInformation
                            {
                                Modifier = "STAFF",
                                DisplayName = item.Subcategory + item.PositionOfficial,
                                DisplayValue = item.NicknameOfficial,
                                Additional = staffs
                            });
                        }
                        //序列化游戏平台
                        string gamePlatforms = null;
                        var isFirst = true;
                        foreach (var item in model.GamePlatforms)
                        {
                            if (item.IsSelected == true)
                            {
                                if (isFirst == true)
                                {
                                    isFirst = false;
                                }
                                else
                                {
                                    gamePlatforms += "、";
                                }
                                gamePlatforms += item.GamePlatformType.ToString();
                            }
                        }
                        //添加基本信息
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间", DisplayValue = model.IssueTime?.ToString("yyyy年M月d日") });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间备注", DisplayValue = model.IssueTimeString });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "原作", DisplayValue = model.Original });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "制作组", DisplayValue = model.ProductionGroup });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "游戏平台", DisplayValue = gamePlatforms });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "引擎", DisplayValue = model.Engine });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行商", DisplayValue = model.Publisher });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行方式", DisplayValue = model.IssueMethod });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "官网", DisplayValue = model.OfficialWebsite });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "Steam平台Id", DisplayValue = model.SteamId });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGame });
                        break;
                    case EntryType.ProductionGroup:
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGroup });
                        break;
                    case EntryType.Role:
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "声优", DisplayValue = model.CV });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性别", DisplayValue = model.Gender.ToString() });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材数据", DisplayValue = model.FigureData });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材(主观)", DisplayValue = model.FigureSubjective });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "生日", DisplayValue = model.Birthday?.ToString("M月d日") });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发色", DisplayValue = model.Haircolor });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "瞳色", DisplayValue = model.Pupilcolor });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性格", DisplayValue = model.Character });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "角色身份", DisplayValue = model.RoleIdentity });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "血型", DisplayValue = model.BloodType });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身高", DisplayValue = model.RoleHeight });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "兴趣", DisplayValue = model.RoleTaste });
                        newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "年龄", DisplayValue = model.RoleAge });


                        break;
                    case EntryType.Staff:
                        break;
                }
                //序列化相关网站

                //加载在基本信息中的网站
                if (string.IsNullOrWhiteSpace(model.WeiboId) == false)
                {
                    model.SocialPlatforms.Add(new SocialPlatform
                    {
                        Name = "微博",
                        Link = "https://weibo.com/u/" + model.WeiboId
                    });
                }
                if (string.IsNullOrWhiteSpace(model.BilibiliId) == false)
                {
                    model.SocialPlatforms.Add(new SocialPlatform
                    {
                        Name = "Bilibili",
                        Link = "https://space.bilibili.com/" + model.BilibiliId
                    });
                }
                if (string.IsNullOrWhiteSpace(model.AcFunId) == false)
                {
                    model.SocialPlatforms.Add(new SocialPlatform
                    {
                        Name = "AcFun",
                        Link = "https://www.acfun.cn/u/" + model.AcFunId
                    });
                }
                if (string.IsNullOrWhiteSpace(model.ZhihuId) == false)
                {
                    model.SocialPlatforms.Add(new SocialPlatform
                    {
                        Name = "知乎",
                        Link = "https://www.zhihu.com/people/" + model.ZhihuId
                    });
                }
                if (string.IsNullOrWhiteSpace(model.AfdianId) == false)
                {
                    model.SocialPlatforms.Add(new SocialPlatform
                    {
                        Name = "爱发电",
                        Link = "https://afdian.net/@" + model.AfdianId
                    });
                }
                if (string.IsNullOrWhiteSpace(model.PixivId) == false)
                {
                    model.SocialPlatforms.Add(new SocialPlatform
                    {
                        Name = "Pixiv",
                        Link = "https://www.pixiv.net/users/" + model.PixivId
                    });
                }
                if (string.IsNullOrWhiteSpace(model.TwitterId) == false)
                {
                    model.SocialPlatforms.Add(new SocialPlatform
                    {
                        Name = "Twitter",
                        Link = "https://twitter.com/" + model.TwitterId
                    });
                }
                if (string.IsNullOrWhiteSpace(model.YouTubeId) == false)
                {
                    model.SocialPlatforms.Add(new SocialPlatform
                    {
                        Name = "YouTube",
                        Link = "https://www.youtube.com/channel/" + model.YouTubeId
                    });
                }
                if (string.IsNullOrWhiteSpace(model.FacebookId) == false)
                {
                    model.SocialPlatforms.Add(new SocialPlatform
                    {
                        Name = "Facebook",
                        Link = "https://www.facebook.com/" + model.FacebookId
                    });
                }


                //遍历一遍当前视图中 相关网站
                foreach (var item in model.SocialPlatforms)
                {
                    newEntry.Information.Add(new BasicEntryInformation
                    {
                        Modifier = "相关网站",
                        DisplayName = item.Name,
                        DisplayValue = item.Link
                    });
                }

                var tempList = newEntry.Information.ToList();
                tempList.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayValue));
                newEntry.Information = tempList;

                //第三步 建立词条图片

                foreach (var item in model.Images)
                {

                    newEntry.Pictures.Add(new EntryPicture
                    {
                        Url = item.Url,
                        Modifier = item.Modifier,
                        Note = item.Note
                    });

                }

                //第四步 建立词条关联信息

                //获取词条文章
                var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();
                var articles = await _articleRepository.GetAll().Where(s => articleIds.Contains(s.Id)).ToListAsync();

                newEntry.Articles = articles;
                newEntry.EntryRelationFromEntryNavigation = entries.Select(s => new EntryRelation
                {
                    ToEntry = s.Id,
                    ToEntryNavigation = s
                }).ToList();

                //循环查找外部链接是否相同
                foreach (var item in model.Others)
                {

                    newEntry.Outlinks.Add(new Outlink
                    {
                        Name = item.DisplayName,
                        BriefIntroduction = item.DisPlayValue,
                        Link = item.Link,
                    });
                }


                //第五步 建立词条主页
                newEntry.MainPage = model.Context;

                //第六步 建立词条标签
                newEntry.Tags = tags;
                var entry = new Entry();
                //获取审核记录
                try
                {
                    entry= await _examineService.AddNewEtryExaminesAsync(newEntry, user, model.Note);
                }
                catch (Exception ex)
                {
                    return new Result { Successful = false, Error = ex.Message };

                }

                //创建词条成功
                return new Result { Successful = true, Error = entry.Id.ToString() };
            }
            catch (Exception ex)
            {
                return new Result { Error = "创建词条的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenEntryAsync(HiddenEntryModel model)
        {
            await _entryRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();
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
            var model = new EditEntryTagViewModel
            {
                Id = entry.Id,
                Name = entry.Name,
                Type = entry.Type
            };
            //获取用户审核记录
            var examine = await _examineService.GetUserEntryActiveExamineAsync(entry.Id, user.Id, Operation.EstablishTags);
            if (examine != null)
            {
                await _entryService.UpdateEntryDataAsync(entry, examine);
            }
            //处理标签
            var tags = new List<RelevancesModel>();
            foreach (var item in entry.Tags)
            {
                tags.Add(new RelevancesModel
                {
                    DisplayName = item.Name
                });
            }

            model.Tags = tags;

            return model;
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
            if (newEntry == null)
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
                if (infor==null)
                {
                    return new Result { Successful = false, Error = "标签 " + item + " 不存在" };
                }
                else
                {
                    tags.Add(infor);
                }
            }
            newEntry.Tags = tags;

            var examines = _entryService.ExaminesCompletion(currentEntry, newEntry);

            if (examines.Any(s => s.Value == Operation.EstablishTags) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EstablishTags);
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, examine.Key);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishTagsAsync(currentEntry, examine.Key as EntryTags);
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, true, resulte, Operation.EstablishTags, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, currentEntry.Id, Operation.EstablishTags);
            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(currentEntry, user, false, resulte, Operation.EstablishRelevances, model.Note);
            }

            return new Result { Successful = true };

        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditEntryPriorityAsync(EditEntryPriorityViewModel model)
        {
            //判断是否为特殊词条
            if (model.Operation == EditEntryPriorityOperation.ClearAllGame)
            {
                await _entryRepository.GetRangeUpdateTable().Where(s => s.Type == EntryType.Game).Set(s => s.Priority, b => 0).ExecuteAsync();
            }
            else if (model.Operation == EditEntryPriorityOperation.None)
            {
                await _entryRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.Priority, b => b.Priority + model.PlusPriority).ExecuteAsync();
            }


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
            model.Examines = await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => s.EntryId == id && s.IsPassed == true), true);
            model.Examines = model.Examines.OrderByDescending(s => s.ApplyTime).ToList();
            //获取编辑状态
            model.State = await _entryService.GetEntryEditState(user, id);

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

            if(contrastExamine == null||currentExamine==null||contrastExamine.EntryId==null||currentExamine.EntryId==null||contrastExamine.EntryId!=currentExamine.EntryId)
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
    }
}
