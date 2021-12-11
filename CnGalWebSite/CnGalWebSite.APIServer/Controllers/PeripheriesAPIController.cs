using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TencentCloud.Ame.V20190916.Models;

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

        public PeripheriesAPIController(IPeripheryService peripheryService,
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
        }


        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PeripheryViewModel>> GetPeripheryViewAsync(long id)
        {
            try
            {
                //获取当前用户ID
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                //通过Id获取周边
                var periphery = await _peripheryRepository.GetAll().AsNoTracking()
                    .Include(s => s.Pictures).Include(s => s.Examines).Include(s => s.Entries)
                    .Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation).ThenInclude(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s=>s.ToPeripheryNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (periphery == null)
                {
                    return NotFound();
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
                        _peripheryService.UpdatePeripheryData(periphery, examine);
                    }
                }

                //建立视图模型
                var model = new PeripheryViewModel
                {
                    Id = periphery.Id,
                    Name = periphery.Name,
                    BriefIntroduction = periphery.BriefIntroduction,
                    MainPicture = _appHelper.GetImagePath(periphery.MainPicture, ""),
                    BackgroundPicture = _appHelper.GetImagePath(periphery.BackgroundPicture, ""),
                    SmallBackgroundPicture = _appHelper.GetImagePath(periphery.SmallBackgroundPicture, ""),
                    Thumbnail = _appHelper.GetImagePath(periphery.Thumbnail, ""),
                    Author = periphery.Author,
                    Material = periphery.Material,
                    CanComment = periphery.CanComment ?? true,
                    ReaderCount = periphery.ReaderCount,
                    CommentCount = periphery.CommentCount,
                    CollectedCount = periphery.CollectedCount,
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

                //判断该周边是否被收集
                if (user != null)
                {
                    model.IsCollected = await _userOwnedPeripheryRepository.GetAll().AnyAsync(s => s.PeripheryId == model.Id && s.ApplicationUserId == user.Id);
                }


                //判断当前是否隐藏
                if (periphery.IsHidden == true)
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

                if (user != null)
                {
                    examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryMain);
                    if (examine != null)
                    {
                        model.MainState = EditState.Preview;
                    }
                }

                //序列化图片列表
                //读取当前用户等待审核的信息
                if (user != null)
                {
                    examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryImages);
                    if (examine != null)
                    {
                        model.ImagesState = EditState.Preview;
                        _peripheryService.UpdatePeripheryData(periphery, examine);
                    }
                }
                //读取词条信息
                var pictures = new List<EntryPicture>();
                foreach (var item in periphery.Pictures)
                {
                    pictures.Add(new EntryPicture
                    {
                        Url = item.Url,
                        Note = item.Note,
                        Modifier = item.Modifier
                    });
                }

                //根据分类来重新排列图片
                var picturesViewModels = new List<PicturesViewModel>
            {
                new PicturesViewModel
                {
                    Modifier=null,
                    Pictures=new List<PicturesAloneViewModel>()
                }
            };

                foreach (var item in pictures)
                {
                    var isAdd = false;
                    foreach (var infor in picturesViewModels)
                    {
                        if (infor.Modifier == item.Modifier)
                        {
                            infor.Pictures.Add(new PicturesAloneViewModel
                            {
                                Note = item.Note,
                                Url = _appHelper.GetImagePath(item.Url, "")
                            });
                            isAdd = true;
                            break;
                        }
                    }

                    if (isAdd == false)
                    {
                        picturesViewModels.Add(new PicturesViewModel
                        {
                            Modifier = item.Modifier,
                            Pictures = new List<PicturesAloneViewModel> {
                            new PicturesAloneViewModel
                            {
                                Note=item.Note,
                                Url=_appHelper.GetImagePath(item.Url, "")
                            }
                        }
                        });
                    }
                }

                //如果所有图片都有分组 则删除默认空分组
                if (picturesViewModels[0].Pictures.Count == 0)
                {
                    picturesViewModels.RemoveAt(0);
                }

                //序列化关联词条列表
                //读取当前用户等待审核的信息
                if (user != null)
                {
                    examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryRelatedEntries);
                    if (examine != null)
                    {
                        model.RelatedEntriesState = EditState.Preview;
                        _peripheryService.UpdatePeripheryData(periphery, examine);
                    }
                }

                //获取所有关联词条
                var entryIds = periphery.Entries.Select(s => s.EntryId).ToArray();

                var entries = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Peripheries).ThenInclude(s => s.Periphery).Where(s => entryIds.Contains(s.Id)).ToListAsync();

                //将当前周边从词条关联里替换 即加载审核预览
                foreach (var item in entries)
                {

                    var temp = item.Peripheries.ToList();
                    temp.RemoveAll(s => s.PeripheryId == id);
                    item.Peripheries = temp;
                    item.Peripheries.Add(new PeripheryRelevanceEntry
                    {
                        PeripheryId = id,
                        Periphery = periphery
                    });
                }
                foreach (var item in entries)
                {
                    model.Entries.Add(_appHelper.GetEntryInforTipViewModel(item));
                }

                //序列化关联周边列表
                //读取当前用户等待审核的信息
                if (user != null)
                {
                    examine = examineQuery.Find(s => s.Operation == Operation.EditPeripheryRelatedPeripheries);
                    if (examine != null)
                    {
                        model.RelatedEntriesState = EditState.Preview;
                        _peripheryService.UpdatePeripheryData(periphery, examine);
                    }
                }
                //获取所有关联周边
                var peripheries = periphery.PeripheryRelationFromPeripheryNavigation.Select(s => s.ToPeripheryNavigation);

                //将当前周边从周边关联里替换 即加载审核预览
                foreach (var item in peripheries)
                {

                    var temp = item.PeripheryRelationFromPeripheryNavigation.ToList();
                    temp.RemoveAll(s => s.ToPeriphery == id);
                    item.PeripheryRelationFromPeripheryNavigation = temp;
                    item.PeripheryRelationFromPeripheryNavigation.Add(new PeripheryRelation
                    {
                        ToPeriphery = id,
                        ToPeripheryNavigation = periphery
                    });
                }
                foreach (var item in peripheries)
                {
                    model.Peripheries.Add(_appHelper.GetPheripheryInforTipViewModel(item));
                }

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
                            model.MainState = EditState.locked;
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
                            model.ImagesState = EditState.locked;
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
                            model.RelatedEntriesState = EditState.locked;
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
                            model.RelatedPeripheriesState = EditState.locked;
                        }
                        else
                        {
                            model.RelatedPeripheriesState = EditState.Normal;
                        }
                    }
                }

                //赋值
                model.Pictures = picturesViewModels;

                //增加阅读人数
                await _peripheryRepository.GetRangeUpdateTable().Where(s => s.Id == id).Set(s => s.ReaderCount, b => b.ReaderCount + 1).ExecuteAsync();


                return model;
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

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
                if(await _peripheryRepository.GetAll().AnyAsync(s=>s.Name==model.Name))
                {
                    return new Result { Successful = false, Error = "该周边的名称与其他周边重复" };
                }
                //预处理 建立词条关联信息
                //判断关联是否存在
                var entryId = new List<int>();

                var entryNames = new List<string>();
                entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
                //确保至少有一个关联词条
                if (entryNames.Any() == false)
                {
                    return new Result { Successful = false, Error = "至少需要关联一个词条" };
                }

                foreach (var item in entryNames)
                {
                    var infor = await _entryRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                    if (infor <= 0)
                    {
                        return new Result { Successful = false, Error = "词条 " + item + " 不存在" };
                    }
                    else
                    {
                        entryId.Add(infor);
                    }
                }
                //删除重复数据
                entryId = entryId.Distinct().ToList();

                //预处理 建立周边关联信息
                //判断关联是否存在
                var peripheryIds = new List<long>();

                var peripheryNames = new List<string>();
                peripheryNames.AddRange(model.Peripheries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));


                foreach (var item in peripheryNames)
                {
                    var infor = await _peripheryRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                    if (infor <= 0)
                    {
                        return new Result { Successful = false, Error = "周边 " + item + " 不存在" };
                    }
                    else
                    {
                        peripheryIds.Add(infor);
                    }
                }
                //删除重复数据
                peripheryIds = peripheryIds.Distinct().ToList();

                //第一步 处理主要信息

                //新建审核数据对象
                var peripheryMain = new PeripheryMain
                {
                    Name = model.Name,
                    BriefIntroduction = model.BriefIntroduction,
                    MainPicture = model.MainPicture,
                    BackgroundPicture = model.BackgroundPicture,
                    SmallBackgroundPicture = model.SmallBackgroundPicture,
                    Thumbnail = model.Thumbnail,
                    Author = model.Author,
                    Material = model.Material,
                    Brand = model.Brand,
                    IndividualParts = model.IndividualParts,
                    IsAvailableItem = model.IsAvailableItem,
                    PageCount = model.PageCount,
                    Price = model.Price,
                    IsReprint = model.IsReprint,
                    Size = model.Size,
                    SongCount = model.SongCount,
                    Type = model.Type,
                    Category = model.Category,
                    SaleLink = model.SaleLink,
                    DisplayName = model.DisplayName,
                };
                //序列化
                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, peripheryMain);
                    resulte = text.ToString();
                }
                //将空文章添加到数据库中 目的是为了获取索引
                var periphery = new Periphery();

                periphery = await _peripheryRepository.InsertAsync(periphery);
                //判断是否是管理员
                if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                {
                    await _examineService.ExamineEditPeripheryMainAsync(periphery, peripheryMain);
                    await _examineService.UniversalCreatePeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryMain, model.Note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, Operation.EditPeripheryMain);
                }
                else
                {
                    await _examineService.UniversalCreatePeripheryExaminedAsync(periphery, user, false, resulte, Operation.EditPeripheryMain, model.Note);
                }

                //第二步 建立词条图片

                //创建审核数据模型
                periphery.Pictures = new List<EntryPicture>();

                var peripheryImages = new PeripheryImages
                {
                    Images = new List<EntryImage>()
                };
                if (model.Images != null)
                {
                    foreach (var item in model.Images)
                    {
                        if (item.Url != "app.png")
                        {
                            //复制到审核的列表中
                            peripheryImages.Images.Add(new EntryImage
                            {
                                Url = item.Url,
                                Note = item.Note,
                                Modifier = item.Modifier,
                                IsDelete = false
                            });
                        }
                    }
                }

                //判断审核是否为空
                if (peripheryImages.Images.Count != 0)
                {
                    //序列化JSON
                    resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, peripheryImages);
                        resulte = text.ToString();
                    }

                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                    {
                        await _examineService.ExamineEditPeripheryImagesAsync(periphery, peripheryImages);
                        await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryImages, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, Operation.EditPeripheryImages);

                    }
                    else
                    {
                        await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, false, resulte, Operation.EditPeripheryImages, model.Note);
                    }
                }


                //第四步 建立周边关联词条信息

                //判断审核是否为空
                if (entryId.Count != 0)
                {
                    //创建审核数据模型
                    var examinedModel = new PeripheryRelatedEntries();

                    foreach (var item in entryId)
                    {
                        examinedModel.Relevances.Add(new PeripheryRelatedEntryAloneModel
                        {
                            IsDelete = false,
                            EntryId = item

                        });
                    }
                    periphery.Entries = new List<PeripheryRelevanceEntry>();
                    //序列化JSON
                    resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, examinedModel);
                        resulte = text.ToString();
                    }
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                    {
                        await _examineService.ExamineEditPeripheryRelatedEntriesAsync(periphery, examinedModel);
                        await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryRelatedEntries, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, Operation.EstablishRelevances);

                    }
                    else
                    {
                        await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, false, resulte, Operation.EditPeripheryRelatedEntries, model.Note);
                    }
                }
                //第五步 建立周边关联周边信息


                //判断审核是否为空
                if (model.Peripheries.Count != 0)
                {   //创建审核数据模型
                    var peripheryRelatedPeripheries = new PeripheryRelatedPeripheries();

                    foreach (var item in peripheryIds)
                    {
                        peripheryRelatedPeripheries.Relevances.Add(new PeripheryRelatedPeripheriesAloneModel
                        {
                            IsDelete = false,
                            PeripheryId = item

                        });
                    }
                    //序列化JSON
                    resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, peripheryRelatedPeripheries);
                        resulte = text.ToString();
                    }
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                    {
                        await _examineService.ExamineEditPeripheryRelatedPeripheriesAsync(periphery, peripheryRelatedPeripheries);
                        await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryRelatedPeripheries, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, Operation.EstablishRelevances);
                    }
                    else
                    {
                        await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, false, resulte, Operation.EditPeripheryRelatedPeripheries, model.Note);
                    }

                }

           
                return new Result { Successful = true, Error = periphery.Id.ToString() };
            }
            catch (Exception ex)
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
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryMain)))
            {
                return NotFound();
            }

            //获取审核记录
            var examine = await _examineService.GetUserPeripheryActiveExamineAsync(periphery.Id, user.Id, Operation.EditPeripheryMain);
            if (examine != null)
            {
                _peripheryService.UpdatePeripheryData(periphery, examine);
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
            if (await _peripheryRepository.GetAll().AnyAsync(s => s.Name == model.Name&&s.Id!=model.Id))
            {
                return new Result { Successful = false, Error = "该周边的名称与其他周边重复" };
            }

            //查找当前周边
            var periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (periphery == null)
            {
                return new Result { Error = $"无法找到ID为{periphery.Id}的周边", Successful = false };
            }

            //判断是否被修改
            if (model.SmallBackgroundPicture != periphery.SmallBackgroundPicture || model.Name != periphery.Name || model.BriefIntroduction != periphery.BriefIntroduction
                || model.MainPicture != periphery.MainPicture || model.Thumbnail != periphery.Thumbnail || model.BackgroundPicture != periphery.BackgroundPicture
                 || model.Material != periphery.Material || model.Author != periphery.Author || model.Brand != periphery.Brand || model.IndividualParts != periphery.IndividualParts
                 || model.IsAvailableItem != periphery.IsAvailableItem || model.IsReprint != periphery.IsReprint || model.PageCount != periphery.PageCount || model.Price != periphery.Price
                 || model.Size != periphery.Size || model.SongCount != periphery.SongCount || model.Type != periphery.Type || model.Category != periphery.Category
                 || model.SaleLink != periphery.SaleLink || model.DisplayName != periphery.DisplayName)
            {
                //添加修改记录
                //新建审核数据对象
                var peripheryMain = new PeripheryMain
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    BriefIntroduction = model.BriefIntroduction,
                    MainPicture = model.MainPicture,
                    BackgroundPicture = model.BackgroundPicture,
                    SmallBackgroundPicture = model.SmallBackgroundPicture,
                    Thumbnail = model.Thumbnail,
                    Author = model.Author,
                    Material = model.Material,
                    Brand = model.Brand,
                    IndividualParts = model.IndividualParts,
                    IsAvailableItem = model.IsAvailableItem,
                    IsReprint = model.IsReprint,
                    PageCount = model.PageCount,
                    Price = model.Price,
                    Size = model.Size,
                    SongCount = model.SongCount,
                    Type = model.Type,
                    Category = model.Category,
                    SaleLink = model.SaleLink,
                };
                //序列化
                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, peripheryMain);
                    resulte = text.ToString();
                }
                //判断是否是管理员
                if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                {
                    await _examineService.ExamineEditPeripheryMainAsync(periphery, peripheryMain);
                    await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryMain, model.Note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, Operation.EditPeripheryMain);
                }
                else
                {
                    await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, false, resulte, Operation.EditPeripheryMain, model.Note);
                }
            }

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
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryImages)))
            {
                return NotFound();
            }


            //获取用户的审核信息
            var examine = await _examineService.GetUserPeripheryActiveExamineAsync(periphery.Id, user.Id, Operation.EditPeripheryImages);
            if (examine != null)
            {
                _peripheryService.UpdatePeripheryData(periphery, examine);
            }
            //处理图片
            var Images = new List<EditImageAloneModel>();
            foreach (var item in periphery.Pictures)
            {
                Images.Add(new EditImageAloneModel
                {
                    Url = _appHelper.GetImagePath(item.Url, ""),
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
            //查找当前周边
            var periphery = await _peripheryRepository.GetAll().Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (periphery == null)
            {
                return new Result { Error = $"无法找到ID为{periphery.Id}的周边", Successful = false };
            }

            //创建审核数据模型

            var peripheryImages = new PeripheryImages
            {
                Images = new List<EntryImage>()
            };
            //先把 当前词条中的图片 都 打上删除标签
            foreach (var item in periphery.Pictures)
            {
                peripheryImages.Images.Add(new EntryImage
                {
                    Url = item.Url,
                    Note = item.Note,
                    Modifier = item.Modifier,
                    IsDelete = true
                });
            }
            //再遍历视图模型中的图片 对应修改
            if (model.Images != null)
            {
                //循环查找是否相同
                foreach (var item in model.Images)
                {
                    var isSame = false;
                    foreach (var infor in periphery.Pictures)
                    {
                        if (item.Url == infor.Url && item.Note == infor.Note && item.Modifier == infor.Modifier)
                        {
                            //如果两次一致 删除上一步中的项目
                            foreach (var temp in peripheryImages.Images)
                            {
                                if (temp.Url == infor.Url && temp.Note == infor.Note && temp.Modifier == infor.Modifier)
                                {
                                    peripheryImages.Images.Remove(temp);
                                    isSame = true;
                                    break;
                                }
                            }
                            isSame = true;
                            break;
                        }
                    }
                    if (isSame == false)
                    {
                        peripheryImages.Images.Add(new EntryImage
                        {
                            Url = _appHelper.GetImagePath(item.Url, ""),
                            Modifier = item.Modifier,
                            Note = item.Note
                        });
                    }
                }
            }

            //判断审核是否为空
            if (peripheryImages.Images.Count == 0)
            {
                return new Result { Successful = true };
            }
            //检查图片链接 是否包含外链
            foreach (var item in peripheryImages.Images)
            {
                if (item.Url.Contains("image.cngal.org") == false && item.Url.Contains("pic.cngal.top") == false)
                {
                    return new Result { Successful = false, Error = "相册中不能添加外部图片：" + item.Url };
                }
            }
            //序列化JSON
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, peripheryImages);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Admin") == true)
            {
                await _examineService.ExamineEditPeripheryImagesAsync(periphery, peripheryImages);
                await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryImages, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, Operation.EditPeripheryImages);

            }
            else
            {
                await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, false, resulte, Operation.EditPeripheryImages, model.Note);
            }
            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditPeripheryRelatedEntriesViewModel>> EditRelatedEntries(long Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var periphery = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (periphery == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryRelatedEntries)))
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
                _peripheryService.UpdatePeripheryData(periphery, examine);

            }
            //处理附加信息
            var roles = new List<RelevancesModel>();
            var staffs = new List<RelevancesModel>();
            var groups = new List<RelevancesModel>();
            var games = new List<RelevancesModel>();

            var entryIds = periphery.Entries.Select(s => s.EntryId).ToList();
            var entries = await _entryRepository.GetAll().Where(s => entryIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Name,
                    s.Type
                })
                .ToListAsync();

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
            //查找当前周边
            var periphery = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (periphery == null)
            {
                return new Result { Error = $"无法找到ID为{periphery.Id}的周边", Successful = false };
            }

            //预处理 建立词条关联信息
            //判断关联是否存在
            var entryIds = new List<int>();

            var entryNames = new List<string>();
            entryNames.AddRange(model.Games.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Groups.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Staffs.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            entryNames.AddRange(model.Roles.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));
            //确保至少有一个关联词条
            if (entryNames.Any() == false)
            {
                return new Result { Successful = false, Error = "至少需要关联一个词条" };
            }

            foreach (var item in entryNames)
            {
                var infor = await _entryRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    return new Result { Successful = false, Error = "词条 " + item + " 不存在" };
                }
                else
                {
                    entryIds.Add(infor);
                }
            }
            //删除重复数据
            entryIds = entryIds.Distinct().ToList();

            //创建审核数据模型
            var examinedModel = new PeripheryRelatedEntries();

            //遍历当前词条数据 打上删除标签
            foreach (var item in periphery.Entries)
            {
                examinedModel.Relevances.Add(new PeripheryRelatedEntryAloneModel
                {
                    EntryId = item.EntryId ?? 0,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in entryIds.Where(s => examinedModel.Relevances.Select(s => s.EntryId).Contains(s) == false))
            {
                examinedModel.Relevances.Add(new PeripheryRelatedEntryAloneModel
                {
                    EntryId = item,
                    IsDelete = false
                });

            }
            //删除不存在的老项目
            examinedModel.Relevances.RemoveAll(s => entryIds.Contains(s.EntryId) && s.IsDelete);

            //判断审核是否为空
            if (examinedModel.Relevances.Count == 0)
            {
                return new Result { Successful = true };
            }

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
                await _examineService.ExamineEditPeripheryRelatedEntriesAsync(periphery, examinedModel);
                await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryRelatedEntries, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, Operation.EstablishRelevances);
            }
            else
            {
                await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, false, resulte, Operation.EditPeripheryRelatedEntries, model.Note);
            }
            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditPeripheryRelatedPeripheriesViewModel>> EditRelatedPeripheries(long Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var periphery = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s=>s.ToPeripheryNavigation).ThenInclude(s=>s.PeripheryRelationFromPeripheryNavigation).FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
            if (periphery == null)
            {
                return NotFound();
            }
            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && (s.Operation == Operation.EditPeripheryRelatedPeripheries)))
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
                _peripheryService.UpdatePeripheryData(periphery, examine);

            }
            //处理附加信息
            var peripheries = new List<RelevancesModel>();

            foreach (var item in periphery.PeripheryRelationFromPeripheryNavigation.Select(s=>s.ToPeripheryNavigation))
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
            //查找当前周边
            var periphery = await _peripheryRepository.GetAll()
                .Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation).ThenInclude(s => s.PeripheryRelationFromPeripheryNavigation)
                .Include(s=>s.PeripheryRelationToPeripheryNavigation)
                .FirstOrDefaultAsync(s => s.Id == model.Id);
            if (periphery == null)
            {
                return new Result { Error = $"无法找到ID为{periphery.Id}的周边", Successful = false };
            }

            //预处理 建立周边关联信息
            //判断关联是否存在
            var peripheryIds = new List<long>();

            var peripheryNames = new List<string>();
            peripheryNames.AddRange(model.Peripheries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));


            foreach (var item in peripheryNames)
            {
                var infor = await _peripheryRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    return new Result { Successful = false, Error = "周边 " + item + " 不存在" };
                }
                else
                {
                    peripheryIds.Add(infor);
                }
            }
            //删除重复数据
            peripheryIds = peripheryIds.Distinct().ToList();

            //创建审核数据模型
            var examinedModel = new PeripheryRelatedPeripheries();

            //遍历当前词条数据 打上删除标签
            foreach (var item in periphery.PeripheryRelationFromPeripheryNavigation.Select(s=>s.ToPeripheryNavigation))
            {
                examinedModel.Relevances.Add(new PeripheryRelatedPeripheriesAloneModel
                {
                    PeripheryId = item.Id,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in peripheryIds.Where(s => examinedModel.Relevances.Select(s => s.PeripheryId).Contains(s) == false))
            {
                examinedModel.Relevances.Add(new PeripheryRelatedPeripheriesAloneModel
                {
                    PeripheryId = item,
                    IsDelete = false
                });

            }
            //删除不存在的老项目
            examinedModel.Relevances.RemoveAll(s => peripheryIds.Contains(s.PeripheryId) && s.IsDelete);

            //判断审核是否为空
            if (examinedModel.Relevances.Count == 0)
            {
                return new Result { Successful = true };
            }

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
                await _examineService.ExamineEditPeripheryRelatedPeripheriesAsync(periphery, examinedModel);
                await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, true, resulte, Operation.EditPeripheryRelatedPeripheries, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, periphery.Id, Operation.EstablishRelevances);
            }
            else
            {
                await _examineService.UniversalEditPeripheryExaminedAsync(periphery, user, false, resulte, Operation.EditPeripheryRelatedPeripheries, model.Note);
            }
            return new Result { Successful = true };

        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<GameOverviewPeripheriesModel>> GetEntryOverviewPeripheries(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            var entry = await _entryRepository.GetAll().Include(s => s.Peripheries).ThenInclude(s => s.Periphery).FirstOrDefaultAsync(s => s.Id == id && s.IsHidden == false);

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
            await _peripheryRepository.GetRangeUpdateTable().Where(s => s.Id == id).Set(s => s.CollectedCount, b => b.CollectedCount + 1).ExecuteAsync();


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
            await _peripheryRepository.GetRangeUpdateTable().Where(s => s.Id == id).Set(s => s.CollectedCount, b => b.CollectedCount - 1).ExecuteAsync();


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
            await _peripheryRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();
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
            var userOwnedPeripheryEntries = await _userOwnedPeripheryRepository.GetAll().Where(s => s.ApplicationUserId == id).Select(s => s.Periphery.Entries).ToListAsync();
            var temp = new List<int?>();
            foreach (var item in userOwnedPeripheryEntries)
            {
                temp.AddRange(item.Select(s => s.EntryId));
            }
            //去重
            temp = temp.Distinct().ToList();
            var ids = temp.Where(s => s != null).Select(s => s.Value).ToList();

            //向数据库查询
            var entries = await _entryRepository.GetAll().Include(s => s.Peripheries).ThenInclude(s => s.Periphery).Where(s => ids.Contains(s.Id)).ToListAsync();

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
            await _peripheryRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.Priority, b => b.Priority + model.PlusPriority).ExecuteAsync();

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
            model.Examines = await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => s.PeripheryId == id && s.IsPassed == true), true);
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


    }
}
