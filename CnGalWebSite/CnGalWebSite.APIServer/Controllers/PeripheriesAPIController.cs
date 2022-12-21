using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
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
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/peripheries/[action]")]
    public class PeripheriesAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<PeripheryRelevanceUser, long> _userOwnedPeripheryRepository;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;
        private readonly IPeripheryService _peripheryService;
        private readonly IEntryService _entryService;
        private readonly IEditRecordService _editRecordService;

        public PeripheriesAPIController(IPeripheryService peripheryService, IEntryService entryService, IEditRecordService editRecordService,
        UserManager<ApplicationUser> userManager, IRepository<PeripheryRelevanceUser, long> userOwnedPeripheryRepository,
         IExamineService examineService, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Periphery, long> peripheryRepository, IRepository<Examine, long> examineRepository)
        {
            _userManager = userManager;
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _appHelper = appHelper;
            _examineService = examineService;
            _peripheryRepository = peripheryRepository;
            _peripheryService = peripheryService;
            _userOwnedPeripheryRepository = userOwnedPeripheryRepository;
            _entryService = entryService;
            _editRecordService = editRecordService;
        }


        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PeripheryViewModel>> GetPeripheryViewAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取周边
            var periphery = await _peripheryRepository.GetAll()
                .Include(s => s.Pictures)
                .Include(s => s.Examines)
                .Include(s => s.RelatedEntries).ThenInclude(s => s.RelatedPeripheries)
                .Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation).ThenInclude(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (periphery == null)
            {
                return NotFound();
            }
            //判断当前是否隐藏
            if (periphery.IsHidden == true)
            {
                if (user == null || await _userManager.IsInRoleAsync(user, "Editor") != true)
                {
                    return NotFound();
                }
            }


            List<Examine> examineQuery = null;
            //读取审核信息
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll()
                               .Where(s => s.PeripheryId == periphery.Id && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EditPeripheryMain || s.Operation == Operation.EditPeripheryImages || s.Operation == Operation.EditPeripheryRelatedEntries || s.Operation == Operation.EditPeripheryRelatedPeripheries))
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
                examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryMain);
                if (examine != null)
                {
                    await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryImages);
                if (examine != null)
                {
                    await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryRelatedEntries);
                if (examine != null)
                {
                    await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryRelatedPeripheries);
                if (examine != null)
                {
                    await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);
                }
            }

            //建立视图模型
            var model = _peripheryService.GetPeripheryViewModel(periphery);


            if (user != null)
            {
                examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryMain);
                if (examine != null)
                {
                    model.MainState = EditState.Preview;
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryImages);
                if (examine != null)
                {
                    model.ImagesState = EditState.Preview;
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryRelatedEntries);
                if (examine != null)
                {
                    model.RelatedEntriesState = EditState.Preview;
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryRelatedPeripheries);
                if (examine != null)
                {
                    model.RelatedEntriesState = EditState.Preview;
                }
            }

            //获取所有关联词条
            var entries = periphery.RelatedEntries;


            //获取所有关联周边
            var peripheries = periphery.PeripheryRelationFromPeripheryNavigation.Select(s => s.ToPeripheryNavigation);

            //获取用户收集的周边
            var ownedPeripheries = new List<long>();
            if (user != null)
            {
                ownedPeripheries = await _userOwnedPeripheryRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.Periphery.IsHidden == false).Select(s => s.PeripheryId).ToListAsync();
            }
            //根据关联词条初始化周边集合
            foreach (var item in entries)
            {
                model.PeripheryOverviewModels.Add(_peripheryService.GetGameOverViewPeripheriesModel(user, item, ownedPeripheries, false));
            }
            //根据关联周边初始化周边集合
            if (periphery.Type != PeripheryType.Set)
            {
                foreach (var item in peripheries)
                {
                    model.PeripheryOverviewModels.Add(_peripheryService.GetGameOverViewPeripheriesModel(user, item, ownedPeripheries, false));
                }
            }
            else
            {
                model.PeripheryOverviewModels.Add(_peripheryService.GetGameOverViewPeripheriesModel(user, periphery, ownedPeripheries, false));
            }



            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.PeripheryId == periphery.Id && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }
            //获取各部分状态
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditPeripheryMain))
                    {
                        model.MainState = EditState.Locked;
                    }
                    else
                    {
                        model.MainState = EditState.Normal;
                    }
                }
                if (model.ImagesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditPeripheryImages))
                    {
                        model.ImagesState = EditState.Locked;
                    }
                    else
                    {
                        model.ImagesState = EditState.Normal;
                    }
                }

                if (model.RelatedEntriesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditPeripheryRelatedEntries))
                    {
                        model.RelatedEntriesState = EditState.Locked;
                    }
                    else
                    {
                        model.RelatedEntriesState = EditState.Normal;
                    }
                }
                if (model.RelatedPeripheriesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditPeripheryRelatedPeripheries))
                    {
                        model.RelatedPeripheriesState = EditState.Locked;
                    }
                    else
                    {
                        model.RelatedPeripheriesState = EditState.Normal;
                    }
                }
            }

            //增加阅读人数
            _peripheryRepository.Clear();
            await _peripheryRepository.GetAll().Where(s => s.Id == id).ExecuteUpdateAsync(s=>s.SetProperty(s => s.ReaderCount, b => b.ReaderCount + 1));

            return model;

        }

        [HttpPost]
        public async Task<ActionResult<Result>> CreatePeripheryAsync(CreatePeripheryViewModel model)
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

                //检查周边是否重名
                if (await _peripheryRepository.GetAll().AnyAsync(s => s.Name == model.Main.Name))
                {
                    return new Result { Successful = false, Error = "该周边的名称与其他周边重复" };
                }
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
                //判断关联是否存在
                var entryIds = new List<int>();

                var entryNames = new List<string>();
                entryNames.AddRange(model.Entries.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Entries.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Entries.Staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Entries.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                try
                {
                    entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
                }
                catch (Exception ex)
                {
                    return new Result { Successful = false, Error = ex.Message };
                }
                //获取词条
                var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();
                var peripheryIds = new List<long>();

                var peripheryNames = new List<string>();
                peripheryNames.AddRange(model.Peripheries.Peripheries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                try
                {
                    peripheryIds = await _peripheryService.GetPeripheryIdsFromNames(peripheryNames);
                }
                catch (Exception ex)
                {
                    return new Result { Successful = false, Error = ex.Message };
                }
                //获取周边
                var peripheries = await _peripheryRepository.GetAll().Where(s => peripheryIds.Contains(s.Id)).ToListAsync();



                var newPeriphery = new Periphery();


                _peripheryService.SetDataFromEditPeripheryMainViewModel(newPeriphery, model.Main);
                _peripheryService.SetDataFromEditPeripheryImagesViewModel(newPeriphery, model.Images);
                _peripheryService.SetDataFromEditPeripheryRelatedEntriesViewModel(newPeriphery, model.Entries, entries);
                _peripheryService.SetDataFromEditPeripheryRelatedPerpheriesViewModel(newPeriphery, model.Peripheries, peripheries);

                var periphery = new Periphery();
                //获取审核记录
                try
                {
                    periphery = await _examineService.AddNewPeripheryExaminesAsync(newPeriphery, user, model.Note);
                }
                catch (Exception ex)
                {
                    return new Result { Successful = false, Error = ex.Message };

                }

                return new Result { Successful = true, Error = periphery.Id.ToString() };
            }
            catch (Exception)
            {
                return new Result { Error = "创建周边的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditPeripheryMainViewModel>> EditMainAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取周边
            var periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == id && s.IsHidden != true);
            if (periphery == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryMain)))
            {
                return NotFound();
            }

            //获取审核记录
            var examine = await _examineService.GetUserPeripheryActiveExamineAsync(periphery.Id, user.Id, Operation.EditPeripheryMain);
            if (examine != null)
            {
                await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);
            }


            var model = new EditPeripheryMainViewModel
            {
                Id = periphery.Id,
                Name = periphery.Name,
                DisplayName = periphery.DisplayName,
                BriefIntroduction = periphery.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(periphery.MainPicture, ""),
                BackgroundPicture = _appHelper.GetImagePath(periphery.BackgroundPicture, ""),
                SmallBackgroundPicture = _appHelper.GetImagePath(periphery.SmallBackgroundPicture, ""),
                Thumbnail = _appHelper.GetImagePath(periphery.Thumbnail, ""),
                Author = periphery.Author,
                Material = periphery.Material,
                Price = periphery.Price,
                PageCount = periphery.PageCount,
                IndividualParts = periphery.IndividualParts,
                Brand = periphery.Brand,
                IsAvailableItem = periphery.IsAvailableItem,
                IsReprint = periphery.IsReprint,
                Size = periphery.Size,
                SongCount = periphery.SongCount,
                Type = periphery.Type,
                Category = periphery.Category,
                SaleLink = periphery.SaleLink,
            };
            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditMainAsync(EditPeripheryMainViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == model.Id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryMain)))
            {
                return new Result { Error = "当前周边该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }
            //检查周边是否重名
            if (await _peripheryRepository.GetAll().AnyAsync(s => s.Name == model.Name && s.Id != model.Id))
            {
                return new Result { Successful = false, Error = "该周边的名称与其他周边重复" };
            }

            //查找当前周边
            var currentPeriphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            var newPeriphery = await _peripheryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.Id);

            if (currentPeriphery == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的周边", Successful = false };
            }

            _peripheryService.SetDataFromEditPeripheryMainViewModel(newPeriphery, model);

            var examines = _peripheryService.ExaminesCompletion(currentPeriphery, newPeriphery);

            if (examines.Any(s => s.Value == Operation.EditPeripheryMain) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EditPeripheryMain);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentPeriphery, user, examine.Key, Operation.EditPeripheryMain, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditPeripheryImagesViewModel>> EditImagesAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取周边
            var periphery = await _peripheryRepository.GetAll().Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == id && s.IsHidden != true);
            if (periphery == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryImages)))
            {
                return NotFound();
            }


            //获取用户的审核信息
            var examine = await _examineService.GetUserPeripheryActiveExamineAsync(periphery.Id, user.Id, Operation.EditPeripheryImages);
            if (examine != null)
            {
                await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);
            }
            //处理图片
            var Images = new List<EditImageAloneModel>();
            foreach (var item in periphery.Pictures)
            {
                Images.Add(new EditImageAloneModel
                {
                    Image = _appHelper.GetImagePath(item.Url, ""),
                    Modifier = item.Modifier,
                    Note = item.Note
                });
            }
            //根据类别生成首个视图模型
            var model = new EditPeripheryImagesViewModel
            {
                Name = periphery.Name,
                Id = id,
                Images = Images
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditImagesAsync(EditPeripheryImagesViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == model.Id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryImages)))
            {
                return new Result { Error = "当前周边该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
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


            //查找当前周边
            var currentPeriphery = await _peripheryRepository.GetAll()
                .Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == model.Id);
            var newPeriphery = await _peripheryRepository.GetAll().AsNoTracking()
                .Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (currentPeriphery == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的周边", Successful = false };
            }

            _peripheryService.SetDataFromEditPeripheryImagesViewModel(newPeriphery, model);

            var examines = _peripheryService.ExaminesCompletion(currentPeriphery, newPeriphery);

            if (examines.Any(s => s.Value == Operation.EditPeripheryImages) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EditPeripheryImages);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentPeriphery, user, examine.Key, Operation.EditPeripheryImages, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditPeripheryRelatedEntriesViewModel>> EditRelatedEntries(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var periphery = await _peripheryRepository.GetAll().AsNoTracking()
                .Include(s => s.RelatedEntries)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsHidden != true);
            if (periphery == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryRelatedEntries)))
            {
                return NotFound();
            }
            var model = new EditPeripheryRelatedEntriesViewModel
            {
                Id = periphery.Id,
                Name = periphery.Name,
            };
            //获取用户的审核信息
            var examine = await _examineService.GetUserPeripheryActiveExamineAsync(periphery.Id, user.Id, Operation.EditPeripheryRelatedEntries);
            if (examine != null)
            {
                await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);

            }
            //处理附加信息
            var roles = new List<RelevancesModel>();
            var staffs = new List<RelevancesModel>();
            var groups = new List<RelevancesModel>();
            var games = new List<RelevancesModel>();

            var entries = periphery.RelatedEntries.Select(s => new
            {
                s.Name,
                s.Type
            })
                .ToList();

            foreach (var item in entries)
            {
                switch (item.Type)
                {
                    case EntryType.Game:
                        games.Add(new RelevancesModel
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
                    case EntryType.Role:
                        roles.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;

                }
            }

            model.Roles = roles;
            model.Staffs = staffs;
            model.Groups = groups;
            model.Games = games;

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditRelatedEntries(EditPeripheryRelatedEntriesViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == model.Id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryRelatedEntries)))
            {
                return new Result { Error = "当前周边该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }

            //预处理 建立词条关联信息
            //判断关联是否存在
            var entryIds = new List<int>();

            var entryNames = new List<string>();
            entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            try
            {
                entryIds = await _entryService.GetEntryIdsFromNames(entryNames);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }
            //获取词条文章
            var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id)).ToListAsync();

            //查找当前周边
            var currentPeriphery = await _peripheryRepository.GetAll()
                .Include(s => s.RelatedEntries).FirstOrDefaultAsync(s => s.Id == model.Id);
            var newPeriphery = await _peripheryRepository.GetAll().AsNoTracking()
                .Include(s => s.RelatedEntries).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (currentPeriphery == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的周边", Successful = false };
            }

            _peripheryService.SetDataFromEditPeripheryRelatedEntriesViewModel(newPeriphery, model, entries);

            var examines = _peripheryService.ExaminesCompletion(currentPeriphery, newPeriphery);

            if (examines.Any(s => s.Value == Operation.EditPeripheryRelatedEntries) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EditPeripheryRelatedEntries);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentPeriphery, user, examine.Key, Operation.EditPeripheryRelatedEntries, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditPeripheryRelatedPeripheriesViewModel>> EditRelatedPeripheries(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var periphery = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation).ThenInclude(s => s.PeripheryRelationFromPeripheryNavigation).FirstOrDefaultAsync(s => s.Id == id && s.IsHidden != true);
            if (periphery == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryRelatedPeripheries)))
            {
                return NotFound();
            }
            var model = new EditPeripheryRelatedPeripheriesViewModel
            {
                Id = periphery.Id,
                Name = periphery.Name,
            };
            //获取用户的审核信息
            var examine = await _examineService.GetUserPeripheryActiveExamineAsync(periphery.Id, user.Id, Operation.EditPeripheryRelatedPeripheries);
            if (examine != null)
            {
                await _peripheryService.UpdatePeripheryDataAsync(periphery, examine);

            }
            //处理附加信息
            var peripheries = new List<RelevancesModel>();

            foreach (var item in periphery.PeripheryRelationFromPeripheryNavigation.Select(s => s.ToPeripheryNavigation))
            {

                peripheries.Add(new RelevancesModel
                {
                    DisplayName = item.Name
                });
            }

            model.Peripheries = peripheries;

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditRelatedPeripheries(EditPeripheryRelatedPeripheriesViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.Id == model.Id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryRelatedPeripheries)))
            {
                return new Result { Error = "当前周边该部分已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
            }

            //预处理 建立周边关联信息
            //判断关联是否存在
            var peripheryIds = new List<long>();

            var peripheryNames = new List<string>();
            peripheryNames.AddRange(model.Peripheries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            try
            {
                peripheryIds = await _peripheryService.GetPeripheryIdsFromNames(peripheryNames);
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }
            //获取词条文章
            var peripheries = await _peripheryRepository.GetAll().Where(s => peripheryIds.Contains(s.Id)).ToListAsync();

            //查找当前周边
            var currentPeriphery = await _peripheryRepository.GetAll()
                .Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation).ThenInclude(s => s.PeripheryRelationFromPeripheryNavigation)
                .Include(s => s.PeripheryRelationToPeripheryNavigation)
                .FirstOrDefaultAsync(s => s.Id == model.Id);
            var newPeriphery = await _peripheryRepository.GetAll().AsNoTracking()
                .Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation).ThenInclude(s => s.PeripheryRelationFromPeripheryNavigation)
                .Include(s => s.PeripheryRelationToPeripheryNavigation)
                .FirstOrDefaultAsync(s => s.Id == model.Id);
            if (currentPeriphery == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的周边", Successful = false };
            }

            _peripheryService.SetDataFromEditPeripheryRelatedPerpheriesViewModel(newPeriphery, model, peripheries);

            var examines = _peripheryService.ExaminesCompletion(currentPeriphery, newPeriphery);

            if (examines.Any(s => s.Value == Operation.EditPeripheryRelatedPeripheries) == false)
            {
                return new Result { Successful = true };
            }
            var examine = examines.FirstOrDefault(s => s.Value == Operation.EditPeripheryRelatedPeripheries);

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(currentPeriphery, user, examine.Key, Operation.EditPeripheryRelatedPeripheries, model.Note);


            return new Result { Successful = true };

        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<GameOverviewPeripheriesModel>> GetEntryOverviewPeripheries(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            var entry = await _entryRepository.GetAll().Include(s => s.RelatedPeripheries).FirstOrDefaultAsync(s => s.Id == id);

            if (entry == null)
            {
                return NotFound("无法找到Id为" + id + "的词条");
            }
            var ownedPeripheries = new List<long>();
            if (user != null)
            {
                ownedPeripheries = await _userOwnedPeripheryRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.Periphery.IsHidden == false).Select(s => s.PeripheryId).ToListAsync();
            }
            return _peripheryService.GetGameOverViewPeripheriesModel(user, entry, ownedPeripheries, true);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> CollectPeriphery(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == id);

            if (periphery == null)
            {
                return new Result { Successful = false, Error = "该周边不存在" };
            }

            if (await _userOwnedPeripheryRepository.GetAll().AnyAsync(s => s.PeripheryId == id && s.ApplicationUserId == user.Id))
            {
                return new Result { Successful = false, Error = "这个周边已经被当前用户收集了" };
            }

            await _userOwnedPeripheryRepository.InsertAsync(new PeripheryRelevanceUser
            {
                StartOwnedTime = DateTime.Now.ToCstTime(),
                ApplicationUserId = user.Id,
                PeripheryId = id
            });

            //增加收集人数
            await _peripheryRepository.GetAll().Where(s => s.Id == id).ExecuteUpdateAsync(s=>s.SetProperty(s => s.CollectedCount, b => b.CollectedCount + 1));


            return new Result { Successful = true };
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> UnCollectPeriphery(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == id);

            if (periphery == null)
            {
                return new Result { Successful = false, Error = "该周边不存在" };
            }

            if (await _userOwnedPeripheryRepository.GetAll().AnyAsync(s => s.PeripheryId == id && s.ApplicationUserId == user.Id) == false)
            {
                return new Result { Successful = false, Error = "这个周边还没有被当前用户收集" };
            }

            await _userOwnedPeripheryRepository.DeleteAsync(s => s.PeripheryId == id && s.ApplicationUserId == user.Id);

            //减少收集人数
            await _peripheryRepository.GetAll().Where(s => s.Id == id).ExecuteUpdateAsync(s=>s.SetProperty(s => s.CollectedCount, b => b.CollectedCount - 1));


            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> RevokeExamine(RevokeExamineModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //查找审核
            var examine = await _examineRepository.FirstOrDefaultAsync(s => s.PeripheryId == model.Id && s.ApplicationUserId == user.Id && s.Operation == model.ExamineType && s.IsPassed == null);
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenPeripheryAsync(HiddenPeripheryModel model)
        {
            await _peripheryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));
            return new Result { Successful = true };
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<List<GameOverviewPeripheriesModel>>> GetUserOverviewPeripheries(string id)
        {
            //获取当前用户ID
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("无法找到该用户");
            }

            //获取用户拥有的周边的关联词条
            var userOwnedPeripheryEntries = await _userOwnedPeripheryRepository.GetAll().AsNoTracking()
                .Where(s => s.ApplicationUserId == id).Select(s => s.Periphery.RelatedEntries)
                .ToListAsync();
            var temp = new List<int>();
            foreach (var item in userOwnedPeripheryEntries)
            {
                temp.AddRange(item.Select(s => s.Id));
            }
            //去重
            temp = temp.Distinct().ToList();
            var ids = temp;

            //向数据库查询
            var entries = await _entryRepository.GetAll().Include(s => s.RelatedPeripheries).Where(s => ids.Contains(s.Id)).ToListAsync();

            //获取用户拥有的周边Id
            var ownedPeripheries = new List<long>();
            if (user != null)
            {
                ownedPeripheries = await _userOwnedPeripheryRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.Periphery.IsHidden == false).Select(s => s.PeripheryId).ToListAsync();
            }


            var model = new List<GameOverviewPeripheriesModel>();
            foreach (var item in entries)
            {
                model.Add(_peripheryService.GetGameOverViewPeripheriesModel(user, item, ownedPeripheries, false));
            }

            return model;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditPeripheryPriorityAsync(EditPeripheryPriorityViewModel model)
        {
            await _peripheryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EditPeripheryInforBindModel>> GetPeripheryEditInforBindModelAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (periphery == null)
            {
                return NotFound("无法找到该周边");
            }
            var model = new EditPeripheryInforBindModel
            {
                Id = id,
                Name = periphery.Name
            };

            //获取编辑记录
            model.Examines = await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => s.PeripheryId == id && (s.IsPassed == true || (user != null && s.IsPassed == null && s.ApplicationUserId == user.Id))), true);
            model.Examines = model.Examines.OrderByDescending(s => s.ApplyTime).ToList();
            //获取编辑状态
            model.State = await _peripheryService.GetPeripheryEditState(user, id);

            return model;
        }

        /// <summary>
        /// 获取输入提示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetAllPeripheriesAsync()
        {
            return await _peripheryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Name).ToArrayAsync();
        }


        /// <summary>
        /// 获取编辑记录概览
        /// </summary>
        /// <param name="contrastId">要对比的编辑记录</param>
        /// <param name="currentId">当前最新的编辑记录</param>
        /// <returns></returns>
        [HttpGet("{contrastId}/{currentId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PeripheryContrastEditRecordViewModel>> GetContrastEditRecordViewsAsync(long contrastId, long currentId)
        {
            if (contrastId > currentId)
            {
                return BadRequest("对比的编辑必须先于当前的编辑");
            }

            var contrastExamine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == contrastId);
            var currentExamine = await _examineRepository.FirstOrDefaultAsync(s => s.Id == currentId);

            if (contrastExamine == null || currentExamine == null || contrastExamine.PeripheryId == null || currentExamine.PeripheryId == null || contrastExamine.PeripheryId != currentExamine.PeripheryId)
            {
                return NotFound("编辑记录Id不正确");
            }

            var currentPeriphery = new Periphery();
            var newPeriphery = new Periphery();

            //获取审核记录
            var examines = await _examineRepository.GetAll().AsNoTracking().Where(s => s.IsPassed == true && s.PeripheryId == currentExamine.PeripheryId).ToListAsync();

            foreach (var item in examines.Where(s => s.Id <= contrastId))
            {
                await _peripheryService.UpdatePeripheryDataAsync(currentPeriphery, item);
            }

            foreach (var item in examines.Where(s => s.Id <= currentId))
            {
                await _peripheryService.UpdatePeripheryDataAsync(newPeriphery, item);
            }

            var result = _peripheryService.ConcompareAndGenerateModel(currentPeriphery, newPeriphery);

            var model = new PeripheryContrastEditRecordViewModel
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
        public async Task<ActionResult<CheckPeripheryIsCollectedModel>> CheckPeripheryIsCollected(long id)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var model = new CheckPeripheryIsCollectedModel();

            //判断该周边是否被收集
            if (user != null)
            {
                model.IsCollected = await _userOwnedPeripheryRepository.GetAll().AnyAsync(s => s.PeripheryId == id && s.ApplicationUserId == user.Id);
            }

            return model;
        }

    }
}
