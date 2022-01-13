using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using CnGalWebSite.DataModel.ViewModel.Search;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TencentCloud.Ame.V20190916.Models;
using Nest;
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
        private readonly IAppHelper _appHelper;
        private readonly IEntryService _entryService;
        private readonly IExamineService _examineService;
        private readonly IPerfectionService _perfectionService;

        public EntriesAPIController(UserManager<ApplicationUser> userManager, IRepository<Article, long> articleRepository,
            IPerfectionService perfectionService, IRepository<Examine, long> examineRepository,
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

            var entry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Relevances).Include(s => s.Examines).Include(s => s.Information).ThenInclude(s => s.Additional).Include(s => s.Tags).Include(s => s.Pictures).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id && x.IsHidden != true);
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
                entry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Disambig)
                    .Include(s => s.Relevances).Include(s => s.Information).ThenInclude(s => s.Additional).Include(s => s.Tags).Include(s => s.Pictures)
                    .AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id);
            }
            catch
            {
                entry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Disambig)
                    .Include(s => s.Relevances).Include(s => s.Information).ThenInclude(s => s.Additional).Include(s => s.Tags).Include(s => s.Pictures)
                    .AsSplitQuery().FirstOrDefaultAsync(x => x.Name == ToolHelper.Base64DecodeName(_id));
            }

            if (entry == null)
            {
                return NotFound();
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
                var examine = examineQuery.Find(s => s.Operation == Operation.EstablishMain);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EstablishMainPage);
                if (examine != null)
                {
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }

            }
            //建立视图模型
            var model = new EntryIndexViewModel
            {
                Id = entry.Id,
                Name = entry.DisplayName ?? entry.Name,
                BriefIntroduction = entry.BriefIntroduction,
                Type = entry.Type,
                Examines = await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => s.EntryId == entry.Id && s.IsPassed == true), true),
                CanComment = entry.CanComment ?? true,
                DisambigId = entry.DisambigId ?? 0,
                DisambigName = entry.Disambig?.Name,
                AnotherName = entry.AnotherName

            };
            if (user != null)
            {
                var examine = examineQuery.Find(s => s.Operation == Operation.EstablishMain);
                if (examine != null)
                {
                    model.MainState = EditState.Preview;
                }
                examine = examineQuery.Find(s => s.Operation == Operation.EstablishMainPage);
                if (examine != null)
                {
                    model.MainPageState = EditState.Preview;
                }

            }

            //判断当前是否隐藏
            if (entry.IsHidden == true)
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


            //初始化图片链接
            model.MainPicture = _appHelper.GetImagePath(entry.MainPicture, (entry.Type == EntryType.Staff || entry.Type == EntryType.Role) ? "" : "app.png");
            model.BackgroundPicture = _appHelper.GetImagePath(entry.BackgroundPicture, "");
            model.Thumbnail = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
            model.SmallBackgroundPicture = _appHelper.GetImagePath(entry.SmallBackgroundPicture, "");


            //初始化主页Html代码
            model.MainPage = _appHelper.MarkdownToHtml(entry.MainPage ?? "");



            //序列化附加信息列表
            //读取当前用户等待审核的信息
            if (user != null)
            {
                var examine = examineQuery.Find(s => s.Operation == Operation.EstablishAddInfor);
                if (examine != null)
                {
                    model.InforState = EditState.Preview;
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
            }
            //读取词条信息
            var tempInformation = entry.Information.Where(s => s.Modifier != "STAFF").ToList();
            //添加别称到附加信息
            if (string.IsNullOrWhiteSpace(entry.AnotherName) == false)
            {
                tempInformation.Add(new BasicEntryInformation
                {
                    DisplayName = "别称",
                    DisplayValue = model.AnotherName,
                    Modifier = "基本信息"
                });
            }

            var information = new List<InformationsModel>();
            var issuleTime = "";
            var issuleTimeString = "";
            if (tempInformation.Count > 0)
            {
                var Publisher = tempInformation.FirstOrDefault(s => s.DisplayName == "发行商")?.DisplayValue;
                if (string.IsNullOrWhiteSpace(Publisher) == false)
                {
                    var temp = Publisher.Replace("，", ",").Replace("、", ",").Split(',');

                    foreach (var item in temp)
                    {
                        model.Publishers.Add(new StaffNameModel
                        {
                            DisplayName = item,
                        });
                    }
                }
                var GroupVaule = tempInformation.FirstOrDefault(s => s.DisplayName == "制作组")?.DisplayValue;
                if (string.IsNullOrWhiteSpace(GroupVaule) == false)
                {
                    var temp = GroupVaule.Replace("，", ",").Replace("、", ",").Split(',');

                    foreach (var item in temp)
                    {
                        model.ProductionGroups.Add(new StaffNameModel
                        {
                            DisplayName = item,
                        });
                    }
                }
              
            }
            foreach (var item in tempInformation)
            {
                //判断
                if (item.DisplayName == "性别")
                {
                    item.DisplayValue = ((GenderType)Enum.Parse(typeof(GenderType), item.DisplayValue)).GetDisplayName();
                }
                else if (item.DisplayName == "Steam平台Id")
                {
                    if (string.IsNullOrWhiteSpace(item.DisplayValue) == false)
                    {
                        try
                        {
                            model.SteamId = int.Parse(item.DisplayValue);
                            continue;
                        }
                        catch { }
                    }

                }
                else if (item.DisplayName == "发行时间")
                {
                    issuleTime = item.DisplayValue;
                }
                else if (item.DisplayName == "发行时间备注")
                {
                    issuleTimeString = item.DisplayValue;
                }
                else if(item.DisplayName=="制作组"|| item.DisplayName == "发行商")
                {
                    continue;
                }

                var isAdd = false;
                //如果信息值为空 则不显示
                if (string.IsNullOrWhiteSpace(item.DisplayValue) == true)
                {
                    continue;
                }
                //遍历信息列表寻找关键词
                foreach (var infor in information)
                {
                    if (infor.Modifier == item.Modifier)
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new KeyValueModel
                        {
                            DisplayName = item.DisplayName,
                            DisplayValue = item.DisplayValue
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new InformationsModel
                    {
                        Modifier = item.Modifier,
                        Informations = new List<KeyValueModel>()
                    };
                    temp.Informations.Add(new KeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue
                    });
                    information.Add(temp);
                }
            }

            //查找发行时间相关
            if (entry.Type == EntryType.Game)
            {
                for (var i = 0; i < information.Count; i++)
                {
                    if (information[i].Modifier == "基本信息")
                    {
                        for (var k = 0; k < information[i].Informations.Count; k++)
                        {
                            var item = information[i].Informations[k];

                            if (item.DisplayName == "发行时间")
                            {
                                if (string.IsNullOrWhiteSpace(issuleTimeString) == false)
                                {
                                    information[i].Informations.Remove(item);
                                    k--;
                                    continue;
                                }
                            }
                            else if (item.DisplayName == "发行时间备注")
                            {
                                if (string.IsNullOrWhiteSpace(issuleTimeString) == false)
                                {
                                    item.DisplayName = "发行时间";
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            //序列化 STAFF
            //先读取词条信息
            var staffInforModel = new List<StaffInforModel>
            {
                new StaffInforModel
                {
                    Modifier = "",
                    StaffList = new List<StaffValue>()
                }
            };
            tempInformation = entry.Information.Where(s => s.Modifier == "STAFF").OrderBy(s => s.Id).ToList();
            foreach (var item in tempInformation)
            {

                var isAdd = false;
                //如果信息值为空 则不显示
                if (string.IsNullOrWhiteSpace(item.DisplayValue) == true)
                {
                    continue;
                }
                //尝试获取staff的显示名称
                var displayName = item.Additional.FirstOrDefault(s => s.DisplayName == "昵称（官方称呼）")?.DisplayValue;//(await _entryRepository.FirstOrDefaultAsync(s => s.Name == item.DisplayValue && s.Type == EntryType.Staff))?.DisplayName ?? item.DisplayValue;
                var mainModifier = "";
                var secordModifier = item.Additional.FirstOrDefault(s => s.DisplayName == "职位（官方称呼）")?.DisplayValue;
                foreach (var infor in item.Additional)
                {
                    if (infor.DisplayName == "子项目")
                    {
                        mainModifier = infor.DisplayValue ?? "";
                    }
                }

                //遍历信息列表寻找 主关键词
                foreach (var infor in staffInforModel)
                {
                    if (infor.Modifier == mainModifier)
                    {
                        //寻找次要关键词
                        foreach (var temp in infor.StaffList)
                        {
                            if (temp.Modifier == secordModifier)
                            {
                                //关键词相同则添加
                                temp.Names.Add(new StaffNameModel
                                {
                                    DisplayName = displayName,
                                });
                                isAdd = true;
                                break;
                            }
                        }
                        //没有找到次要关键词 则新建次要关键词
                        if (isAdd == false)
                        {
                            //没有找到关键词 则新建关键词
                            var temp = new StaffValue
                            {
                                Modifier = secordModifier,
                                Names = new List<StaffNameModel>()
                            };
                            temp.Names.Add(new StaffNameModel
                            {
                                DisplayName = displayName,
                            });
                            infor.StaffList.Add(temp);
                            isAdd = true;
                        }
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到主关键词 则新建关键词
                    var temp = new StaffInforModel
                    {
                        Modifier = mainModifier,
                        StaffList = new List<StaffValue>()
                    };
                    temp.StaffList.Add(new StaffValue
                    {
                        Modifier = secordModifier,
                        Names = new List<StaffNameModel>()
                    });
                    temp.StaffList[0].Names.Add(new StaffNameModel
                    {
                        DisplayName = displayName,
                    });
                    staffInforModel.Add(temp);
                }
            }

            //如果所有staff都有分组 则删除默认空分组
            if (staffInforModel[0].StaffList.Count == 0)
            {
                staffInforModel.RemoveAt(0);
            }


            //为Staff匹配Id
            List<StaffNameModel> staffRealNames = new List<StaffNameModel>();
            staffRealNames.AddRange(model.Publishers);
            staffRealNames.AddRange(model.ProductionGroups);
            foreach (var item in staffInforModel)
            {
                foreach (var temp in item.StaffList)
                {
                    staffRealNames.AddRange(temp.Names);
                }
            }
            
            var staffRealIds = await _entryRepository.GetAll().AsNoTracking().Where(s => staffRealNames.Select(s=>s.DisplayName).Contains(s.Name)).Select(s => new
            {
                s.DisplayName,
                s.Name,
                s.Id
            }).ToListAsync();

            foreach(var item in staffRealIds)
            {
                var temp= staffRealNames.Where(s => s.DisplayName == item.Name).ToList();
                foreach(var infor in temp)
                {
                    infor.DisplayName = item.DisplayName;
                    infor.Id=item.Id;
                }
            }

            //序列化图片列表
            //读取当前用户等待审核的信息
            if (user != null)
            {
                var examine = examineQuery.Find(s => s.Operation == Operation.EstablishImages);
                if (examine != null)
                {
                    model.ImagesState = EditState.Preview;
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
            }
            //读取词条信息
            var pictures = new List<EntryPicture>();
            foreach (var item in entry.Pictures)
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

            //序列化相关性列表
            //读取当前用户等待审核的信息
            if (user != null)
            {
                var examine = examineQuery.Find(s => s.Operation == Operation.EstablishRelevances);
                if (examine != null)
                {
                    model.RelevancesState = EditState.Preview;
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
            }
            //读取词条信息
            var relevances = new List<RelevancesViewModel>();
            foreach (var item in entry.Relevances)
            {
                var isAdd = false;
                //如果显示名称和显示值都为空 不显示此词条 应该删除 但是批量导入数据格式默认正确不进行检查
                if (string.IsNullOrWhiteSpace(item.DisplayName) && string.IsNullOrWhiteSpace(item.DisplayValue))
                {
                    continue;
                }
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


            //序列化标签列表
            //读取当前用户等待审核的信息
            if (user != null)
            {
                var examine = examineQuery.Find(s => s.Operation == Operation.EstablishTags);
                if (examine != null)
                {
                    model.TagState = EditState.Preview;
                    await _entryService.UpdateEntryDataAsync(entry, examine);
                }
            }
            //读取词条信息
            var tags = new List<TagsViewModel>();
            foreach (var item in entry.Tags)
            {
                tags.Add(new TagsViewModel { Name = item.Name,Id=item.Id });
            }

            //加载附加信息 关联词条获取
            var roleInforModel = new List<EntryInforTipViewModel>();
            var newsModel = new List<NewsModel>();
            var staffGames = new List<EntryInforTipViewModel>();
            var relevancesEntry = new List<EntryInforTipViewModel>();
            var relevanceArticle = new List<ArticleInforTipViewModel>();
            var relevanceOther = new List<RelevancesKeyValueModel>();

            for (var i = 0; i < relevances.Count; i++)
            {
                switch (relevances[i].Modifier)
                {
                    case "角色":
                        try
                        {
                            if (entry.Type != EntryType.Game)
                            {
                                //一次性查找数据
                                var tempStrings = relevances[i].Informations.Select(s => s.DisplayName).ToArray();
                                var tempDatas = await _entryRepository.GetAll().Include(s => s.Information).ThenInclude(s => s.Additional).Include(s => s.Relevances).Where(s => tempStrings.Any(n => n == s.Name)).AsNoTracking().ToListAsync();

                                foreach (var infor in tempDatas)
                                {
                                    var role = _appHelper.GetEntryInforTipViewModel(infor);
                                    if(entry.Type==EntryType.Staff)
                                    {
                                        role.AddInfors.RemoveAll(s => s.Modifier == "配音");
                                    }
                                    relevancesEntry.Add(role);
                                }
                            }
                            else
                            {
                                //一次性查找数据
                                var tempStrings = relevances[i].Informations.Select(s => s.DisplayName).ToArray();
                                var tempDatas = await _entryRepository.GetAll().Include(s => s.Information).Where(s => tempStrings.Any(n => n == s.Name)).AsNoTracking().ToListAsync();

                                foreach (var infor in tempDatas)
                                {
                                    //获取角色词条
                                    var role = _appHelper.GetEntryInforTipViewModel(infor);
                                    role.AddInfors.Clear();
                                    //查找配音
                                    var roleCV = infor.Information.FirstOrDefault(s => s.DisplayName == "声优")?.DisplayValue;
                                    if(string.IsNullOrWhiteSpace( roleCV )==false)
                                    {
                                        var temp = roleCV.Replace("，", ",").Replace("、", ",").Split(',');
                                        role.AddInfors.Add(new EntryInforTipAddInforModel
                                        {
                                            Modifier = "配音",
                                            Contents = temp.ToList()
                                        });
                                    }
                                 
                                    roleInforModel.Add(role);
                                }
                            }

                        }
                        catch
                        {

                        }

                        break;
                    case "STAFF":
                        try
                        {
                            if (entry.Type != EntryType.Game)
                            {
                                //一次性查找数据
                                var tempStrings = relevances[i].Informations.Select(s => s.DisplayName).ToArray();
                                var tempDatas = await _entryRepository.GetAll().Include(s => s.Information).ThenInclude(s => s.Additional).Include(s => s.Relevances).Where(s => tempStrings.Any(n => n == s.Name)).AsNoTracking().ToListAsync();

                                foreach (var infor in tempDatas)
                                {
                                    relevancesEntry.Add(_appHelper.GetEntryInforTipViewModel(infor));
                                }
                            }
                        }
                        catch
                        {

                        }

                        break;
                    case "制作组":
                        try
                        {
                            //一次性查找数据
                            var tempStrings2 = relevances[i].Informations.Select(s => s.DisplayName).ToArray();
                            var tempDatas2 = await _entryRepository.GetAll().Include(s => s.Information).ThenInclude(s => s.Additional).Include(s => s.Relevances).Where(s => tempStrings2.Any(n => n == s.Name)).AsNoTracking().ToListAsync();

                            foreach (var infor_ in tempDatas2)
                            {
                                relevancesEntry.Add(_appHelper.GetEntryInforTipViewModel(infor_));
                            }
                        }
                        catch
                        {

                        }

                        break;
                    case "游戏":
                        try
                        {
                            if (entry.Type == EntryType.Staff)
                            {
                                //一次性查找数据
                                var tempStrings = relevances[i].Informations.Select(s => s.DisplayName).ToArray();
                                var tempDatas = await _entryRepository.GetAll().Include(s => s.Information).Where(s => tempStrings.Any(n => n == s.Name)).AsNoTracking().ToListAsync();


                                foreach (var infor in tempDatas)
                                {
                                    //获取角色词条
                                    var staffGame = _appHelper.GetEntryInforTipViewModel(infor);
                                    staffGame.AddInfors.Clear();
                                    //查找担任过的职位
                                    var tempStaffs = infor.Information.Where(s => s.Modifier == "STAFF" && s.DisplayValue == entry.Name);
                                    if(tempStaffs.Any())
                                    {
                                        var inforPositions = new List<string>();
                                        foreach (var roleInfor in tempStaffs)
                                        {
                                            inforPositions.Add(roleInfor.DisplayName);
                                        }

                                        staffGame.AddInfors.Add(new EntryInforTipAddInforModel
                                        {
                                            Modifier = "职位",
                                            Contents = inforPositions
                                        });
                                    }
                                   
                                    staffGames.Add(staffGame);
                                }
                            }
                            else
                            {
                                //一次性查找数据
                                var tempStrings = relevances[i].Informations.Select(s => s.DisplayName).ToArray();
                                var tempDatas = await _entryRepository.GetAll().Include(s=>s.Information).ThenInclude(s=>s.Additional).Include(s=>s.Relevances).Where(s => tempStrings.Any(n => n == s.Name)).AsNoTracking().ToListAsync();

                                foreach (var infor in tempDatas)
                                {
                                    relevancesEntry.Add(_appHelper.GetEntryInforTipViewModel(infor));
                                }
                            }

                        }
                        catch
                        {

                        }

                        break;
                    case "文章":
                        try
                        {
                            //一次性查找数据
                            var tempStrings1 = relevances[i].Informations.Select(s => s.DisplayName).ToArray();
                            var tempDatas1 = await _articleRepository.GetAll().Include(s => s.CreateUser).Where(s => tempStrings1.Any(n => n == s.Name)).AsNoTracking().ToListAsync();

                            foreach (var infor1 in tempDatas1)
                            {
                                relevanceArticle.Add(_appHelper.GetArticleInforTipViewModel(infor1));
                            }
                        }
                        catch
                        {

                        }

                        break;
                    case "动态":
                        try
                        {
                            foreach (var infor in relevances[i].Informations)
                            {
                                //获取 动态 填充信息
                                var temp = await _articleRepository.GetAll().Include(s => s.CreateUser).Include(s => s.Relevances).FirstOrDefaultAsync(x => x.Name == infor.DisplayName);
                                if (temp != null)
                                {
                                    var temp1 = new NewsModel
                                    {
                                        Title = temp.DisplayName ?? temp.Name,
                                        BriefIntroduction = temp.BriefIntroduction,
                                        Link =temp.OriginalLink ?? ("/articles/index/" + temp.Id),
                                        HappenedTime = temp.RealNewsTime ?? temp.CreateTime,
                                        NewsType = temp.NewsType ?? "动态",
                                    };

                                    var infor1 = temp.Relevances.FirstOrDefault(s => s.Modifier == "制作组");
                                    if (infor == null)
{
                                        infor1 = temp.Relevances.FirstOrDefault(s => s.Modifier == "STAFF");
                                        if (infor == null)
{
                                            infor1 = temp.Relevances.FirstOrDefault(s => s.Modifier == "游戏");
                                            if (infor == null)
                                            {
                                                infor1 = temp.Relevances.FirstOrDefault(s => s.Modifier == "角色");
                                            }
                                        }
                                    }
                                    if (infor1 != null)
                                    {
                                        var group = await _entryRepository.FirstOrDefaultAsync(s => s.Name == infor1.DisplayName);
                                        if (group != null)
                                        {
                                            temp1.Image = string.IsNullOrWhiteSpace(group.Thumbnail) ? _appHelper.GetImagePath(temp.CreateUser.PhotoPath, "user.png") : _appHelper.GetImagePath(group.Thumbnail, "user.png");
                                            temp1.GroupName = group.DisplayName ?? group.Name;
                                            temp1.GroupId = group.Id;
                                        }
                                        else
                                        {
                                            temp1.Image = _appHelper.GetImagePath(temp.CreateUser.PhotoPath, "user.png");
                                            temp1.GroupName = temp.CreateUser.UserName;
                                        }
                                    }
                                    else
                                    {
                                        temp1.Image = _appHelper.GetImagePath(temp.CreateUser.PhotoPath, "user.png");
                                        temp1.GroupName = temp.CreateUser.UserName;
                                    }

                                    newsModel.Add(temp1);
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }

                        break;
                    default:
                        try
                        {
                            foreach (var infor in relevances[i].Informations)
                            {
                                relevanceOther.Add(new RelevancesKeyValueModel
                                {
                                    DisplayName = infor.DisplayName,
                                    DisplayValue = infor.DisplayValue,
                                    Link = infor.Link,
                                    Image = infor.Image
                                });
                            }
                        }
                        catch
                        {

                        }

                        break;
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


            //赋值
            model.Information = information;
            model.Pictures = picturesViewModels;
            model.ArticleRelevances = relevanceArticle;
            model.EntryRelevances = relevancesEntry;
            model.OtherRelevances = relevanceOther;
            model.Tags = tags;
            model.Roles = roleInforModel;
            model.StaffGames = staffGames;
            newsModel.Sort((NewsModel a, NewsModel b) => { return a.HappenedTime > b.HappenedTime ? 1 : (a.HappenedTime < b.HappenedTime ? -1 : 0); });
            model.NewsOfEntry = newsModel;

            model.Staffs = staffInforModel;
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

            //查找当前词条
            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (entry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }
            //判断名称是否重复
            if (model.Name != entry.Name && await _entryRepository.FirstOrDefaultAsync(s => s.Name == model.Name && s.Id != model.Id) != null)
            {
                return new Result { Error = "该词条的名称与其他词条重复", Successful = false };
            }
            //判断是否被修改
            if (model.SmallBackgroundPicture != entry.SmallBackgroundPicture || model.Name != entry.Name || model.BriefIntroduction != entry.BriefIntroduction
                || model.MainPicture != entry.MainPicture || model.Thumbnail != entry.Thumbnail || model.BackgroundPicture != entry.BackgroundPicture
                || model.Type != entry.Type || model.DisplayName != entry.DisplayName || model.AnotherName != entry.AnotherName)
            {
                //添加修改记录
                //新建审核数据对象
                var entryMain = new EntryMain
                {
                    Name = model.Name,
                    BriefIntroduction = model.BriefIntroduction,
                    MainPicture = model.MainPicture,
                    Thumbnail = model.Thumbnail,
                    BackgroundPicture = model.BackgroundPicture,
                    Type = model.Type,
                    DisplayName = model.DisplayName,
                    SmallBackgroundPicture = model.SmallBackgroundPicture,
                    AnotherName = model.AnotherName
                };
                //序列化
                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, entryMain);
                    resulte = text.ToString();
                }
                //判断是否是管理员
                if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                {
                    await _examineService.ExamineEstablishMainAsync(entry, entryMain);
                    await _examineService.UniversalEditExaminedAsync(entry, user, true, resulte, Operation.EstablishMain, model.Note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishMain);
                }
                else
                {
                    await _examineService.UniversalEditExaminedAsync(entry, user, false, resulte, Operation.EstablishMain, model.Note);
                }
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
                            var socialPlatform = new SocialPlatform
                            {
                                Name = item.DisplayName,
                                Link = item.DisplayValue
                            };

                            model.SocialPlatforms.Add(socialPlatform);

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
                            switch (item.DisplayName)
                            {
                                case "昵称（官方称呼）":
                                    model.Nickname = item.DisplayValue;
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
            var entry = await _entryRepository.GetAll()
                .Include(s => s.Relevances)
              .Include(s => s.Information)
                .ThenInclude(s => s.Additional)
              .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }
            //根据类别进行序列化操作
            var basics1 = new List<BasicEntryInformation>();
            var basics = new List<BasicEntryInformation_>();
            switch (model.Type)
            {
                case EntryType.Game:

                    if (model.Staffs != null)
                    {
                        //先读取当前词条 把staff信息全部删除
                        foreach (var item in entry.Information)
                        {
                            if (item.Modifier == "STAFF")
                            {
                                var additional_s = new List<BasicEntryInformationAdditional_>();
                                foreach (var temp in item.Additional)
                                {
                                    additional_s.Add(new BasicEntryInformationAdditional_ { DisplayName = temp.DisplayName, DisplayValue = temp.DisplayValue, IsDelete = true });
                                }
                                basics.Add(new BasicEntryInformation_ { Modifier = "STAFF", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = true, Additional = additional_s });
                            }
                        }
                        //遍历一遍当前视图中staffs 
                        foreach (var item in model.Staffs)
                        {
                            var isSame = false;
                            for (var i = 0; i < basics.Count; i++)
                            {
                                var infor = basics[i];
                                if (item.Subcategory + item.PositionOfficial == infor.DisplayName && item.NicknameOfficial == infor.DisplayValue && infor.Modifier == "STAFF")
                                {
                                    //如果两次一致 继续查看附加信息是否一致
                                    infor.IsDelete = false;
                                    var allSame = true;

                                    #region 查找职位（官方称呼）是否一致
                                    var temp = infor.Additional.Find(s => s.DisplayName == "职位（官方称呼）");
                                    if (temp == null)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "职位（官方称呼）", DisplayValue = item.PositionOfficial });
                                    }
                                    else if (temp.DisplayValue != item.PositionOfficial)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "职位（官方称呼）", DisplayValue = item.PositionOfficial });
                                        allSame = false;
                                    }
                                    else
                                    {
                                        infor.Additional.Remove(temp);
                                    }

                                    temp = infor.Additional.Find(s => s.DisplayName == "昵称（官方称呼）");
                                    if (temp == null)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "昵称（官方称呼）", DisplayValue = item.NicknameOfficial });
                                    }
                                    else if (temp.DisplayValue != item.NicknameOfficial)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "昵称（官方称呼）", DisplayValue = item.NicknameOfficial });
                                        allSame = false;
                                    }
                                    else
                                    {
                                        infor.Additional.Remove(temp);
                                    }


                                    temp = infor.Additional.Find(s => s.DisplayName == "职位（通用）");
                                    if (temp == null)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "职位（通用）", DisplayValue = item.PositionGeneral.ToString() });
                                    }
                                    else if (temp.DisplayValue != item.PositionGeneral.ToString())
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "职位（通用）", DisplayValue = item.PositionGeneral.ToString() });
                                        allSame = false;
                                    }
                                    else
                                    {
                                        infor.Additional.Remove(temp);
                                    }


                                    temp = infor.Additional.Find(s => s.DisplayName == "角色");
                                    if (temp == null)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "角色", DisplayValue = item.Role });
                                    }
                                    else if (temp.DisplayValue != item.Role)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "角色", DisplayValue = item.Role });
                                        allSame = false;
                                    }
                                    else
                                    {
                                        infor.Additional.Remove(temp);
                                    }


                                    temp = infor.Additional.Find(s => s.DisplayName == "隶属组织");
                                    if (temp == null)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "隶属组织", DisplayValue = item.SubordinateOrganization });
                                    }
                                    else if (temp.DisplayValue != item.SubordinateOrganization)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "隶属组织", DisplayValue = item.SubordinateOrganization });
                                        allSame = false;
                                    }
                                    else
                                    {
                                        infor.Additional.Remove(temp);
                                    }


                                    temp = infor.Additional.Find(s => s.DisplayName == "子项目");
                                    if (temp == null)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "子项目", DisplayValue = item.Subcategory });
                                    }
                                    else if (temp.DisplayValue != item.Subcategory)
                                    {
                                        infor.Additional.Add(new BasicEntryInformationAdditional_ { DisplayName = "子项目", DisplayValue = item.Subcategory });
                                        allSame = false;
                                    }
                                    else
                                    {
                                        infor.Additional.Remove(temp);
                                    }
                                    #endregion

                                    if (allSame == true)
                                    {
                                        basics.Remove(infor);
                                    }
                                    isSame = true;
                                    break;
                                }
                            }
                            if (isSame == false)
                            {
                                var staffs = new List<BasicEntryInformationAdditional_>
                                {
                                    new BasicEntryInformationAdditional_ { DisplayName = "职位（官方称呼）", DisplayValue = item.PositionOfficial },
                                    new BasicEntryInformationAdditional_ { DisplayName = "昵称（官方称呼）", DisplayValue = item.NicknameOfficial },
                                    new BasicEntryInformationAdditional_ { DisplayName = "职位（通用）", DisplayValue = item.PositionGeneral.ToString() },
                                    new BasicEntryInformationAdditional_ { DisplayName = "角色", DisplayValue = item.Role },
                                    new BasicEntryInformationAdditional_ { DisplayName = "隶属组织", DisplayValue = item.SubordinateOrganization },
                                    new BasicEntryInformationAdditional_ { DisplayName = "子项目", DisplayValue = item.Subcategory }
                                };
                                basics.Add(new BasicEntryInformation_ { Modifier = "STAFF", DisplayName = item.Subcategory + item.PositionOfficial, DisplayValue = item.NicknameOfficial, IsDelete = false, Additional = staffs });
                            }
                        }
                        //再遍历视图模型 添加
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
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间", DisplayValue = model.IssueTime?.ToString("yyyy年M月d日") });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间备注", DisplayValue = model.IssueTimeString });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "原作", DisplayValue = model.Original });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "制作组", DisplayValue = model.ProductionGroup });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "游戏平台", DisplayValue = gamePlatforms });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "引擎", DisplayValue = model.Engine });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行商", DisplayValue = model.Publisher });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行方式", DisplayValue = model.IssueMethod });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "官网", DisplayValue = model.OfficialWebsite });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "Steam平台Id", DisplayValue = model.SteamId });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGame });
                    //循环添加 删除标签 到原有信息列表的每一各项目中
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            basics.Add(new BasicEntryInformation_ { Modifier = "基本信息", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = true });
                        }
                    }
                    //循环查找是否相同
                    foreach (var item in basics1)
                    {
                        var isSame = false;
                        foreach (var infor in entry.Information)
                        {
                            if (infor.DisplayName + infor.DisplayValue == item.DisplayName + item.DisplayValue && infor.Modifier == "基本信息")
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in basics)
                                {
                                    if (temp.Modifier == infor.Modifier && temp.DisplayName == infor.DisplayName && temp.DisplayValue == infor.DisplayValue)
                                    {
                                        basics.Remove(temp);
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
                            basics.Add(new BasicEntryInformation_ { Modifier = "基本信息", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = false, });
                        }
                    }
                    break;
                case EntryType.ProductionGroup:
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGroup });
                    //循环添加 删除标签 到原有信息列表的每一各项目中
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            basics.Add(new BasicEntryInformation_ { Modifier = "基本信息", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = true });
                        }
                    }
                    //循环查找是否相同
                    foreach (var item in basics1)
                    {
                        var isSame = false;
                        foreach (var infor in entry.Information)
                        {
                            if (infor.DisplayName + infor.DisplayValue == item.DisplayName + item.DisplayValue && infor.Modifier == "基本信息")
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in basics)
                                {
                                    if (temp.Modifier == infor.Modifier && temp.DisplayName == infor.DisplayName && temp.DisplayValue == infor.DisplayValue)
                                    {
                                        basics.Remove(temp);
                                        break;
                                    }
                                }
                                isSame = true;
                                break;
                            }
                        }
                        if (isSame == false)
                        {
                            basics.Add(new BasicEntryInformation_ { Modifier = "基本信息", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = false, });
                        }
                    }
                    break;
                case EntryType.Role:
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "声优", DisplayValue = model.CV });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性别", DisplayValue = model.Gender.ToString() });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材数据", DisplayValue = model.FigureData });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材(主观)", DisplayValue = model.FigureSubjective });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "生日", DisplayValue = model.Birthday?.ToString("M月d日") });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发色", DisplayValue = model.Haircolor });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "瞳色", DisplayValue = model.Pupilcolor });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性格", DisplayValue = model.Character });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "角色身份", DisplayValue = model.RoleIdentity });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "血型", DisplayValue = model.BloodType });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身高", DisplayValue = model.RoleHeight });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "兴趣", DisplayValue = model.RoleTaste });
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "年龄", DisplayValue = model.RoleAge });

                    //循环添加 删除标签 到原有信息列表的每一各项目中
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            basics.Add(new BasicEntryInformation_ { Modifier = "基本信息", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = true });
                        }
                    }
                    //循环查找是否相同
                    foreach (var item in basics1)
                    {
                        var isSame = false;
                        foreach (var infor in entry.Information)
                        {
                            if (infor.DisplayName + infor.DisplayValue == item.DisplayName + item.DisplayValue && infor.Modifier == "基本信息")
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in basics)
                                {
                                    if (temp.Modifier == infor.Modifier && temp.DisplayName == infor.DisplayName && temp.DisplayValue == infor.DisplayValue)
                                    {
                                        basics.Remove(temp);
                                        break;
                                    }
                                }
                                isSame = true;
                                break;
                            }
                        }
                        if (isSame == false)
                        {
                            basics.Add(new BasicEntryInformation_ { Modifier = "基本信息", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = false, });
                        }
                    }
                    break;
                case EntryType.Staff:
                    basics1.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "昵称（官方称呼）", DisplayValue = model.Nickname });
                    //循环添加 删除标签 到原有信息列表的每一各项目中
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            basics.Add(new BasicEntryInformation_ { Modifier = "基本信息", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = true });
                        }
                    }
                    //循环查找是否相同
                    foreach (var item in basics1)
                    {
                        var isSame = false;
                        foreach (var infor in entry.Information)
                        {
                            if (infor.DisplayName + infor.DisplayValue == item.DisplayName + item.DisplayValue && infor.Modifier == "基本信息")
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp in basics)
                                {
                                    if (temp.Modifier == infor.Modifier && temp.DisplayName == infor.DisplayName && temp.DisplayValue == infor.DisplayValue)
                                    {
                                        basics.Remove(temp);
                                        break;
                                    }
                                }
                                isSame = true;
                                break;
                            }
                        }
                        if (isSame == false)
                        {
                            basics.Add(new BasicEntryInformation_ { Modifier = "基本信息", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = false, });
                        }
                    }
                    break;
            }
            //序列化相关网站
            if (model.SocialPlatforms != null)
            {
                //先读取当前词条 把 相关网站 全部删除
                foreach (var item in entry.Information)
                {
                    if (item.Modifier == "相关网站")
                    {
                        basics.Add(new BasicEntryInformation_ { Modifier = "相关网站", DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = true });
                    }
                }
                //遍历一遍当前视图中 相关网站
                foreach (var item in model.SocialPlatforms)
                {
                    var isSame = false;
                    foreach (var infor in basics)
                    {
                        if (item.Name == infor.DisplayName && item.Link == infor.DisplayValue && infor.Modifier == "相关网站")
                        {
                            //如果两次一致 删除上一步中的项目
                            foreach (var temp in basics)
                            {
                                if (temp.Modifier == infor.Modifier && temp.DisplayName == infor.DisplayName && temp.DisplayValue == infor.DisplayValue)
                                {
                                    basics.Remove(temp);
                                    break;
                                }
                            }
                            isSame = true;
                            break;
                        }
                    }
                    if (isSame == false)
                    {
                        var staffs = new List<BasicEntryInformationAdditional_>();
                        basics.Add(new BasicEntryInformation_ { Modifier = "相关网站", DisplayName = item.Name, DisplayValue = item.Link, IsDelete = false });
                    }
                }
            }

            //新建审核数据对象
            var entryAddInfor = new EntryAddInfor
            {
                Information = basics
            };
            //如果 DisplayValue 为null 则不添加审核记录
            for (var i = 0; i < entryAddInfor.Information.Count; i++)
            {
                if (entryAddInfor.Information[i].IsDelete == false && entryAddInfor.Information[i].DisplayValue == null)
                {
                    entryAddInfor.Information.RemoveAt(i);
                    i--;
                }
            }
            //判断审核列表是否为空
            if (entryAddInfor.Information.Count == 0)
            {
                return new Result { Successful = true };
            }
            //序列化
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, entryAddInfor);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishAddInforAsync(entry, entryAddInfor);
                await _examineService.UniversalEditExaminedAsync(entry, user, true, resulte, Operation.EstablishAddInfor, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishAddInfor);

            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(entry, user, false, resulte, Operation.EstablishAddInfor, model.Note);
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
            //查找词条
            var entry = await _entryRepository.GetAll()
              .Include(s => s.Pictures)
              .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }

            //创建审核数据模型

            var entryImages = new EntryImages
            {
                Images = new List<EntryImage>()
            };
            //先把 当前词条中的图片 都 打上删除标签
            foreach (var item in entry.Pictures)
            {
                entryImages.Images.Add(new EntryImage
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
                    foreach (var infor in entry.Pictures)
                    {
                        if (item.Url == infor.Url && item.Note == infor.Note && item.Modifier == infor.Modifier)
                        {
                            //如果两次一致 删除上一步中的项目
                            foreach (var temp in entryImages.Images)
                            {
                                if (temp.Url == infor.Url && temp.Note == infor.Note && temp.Modifier == infor.Modifier)
                                {
                                    entryImages.Images.Remove(temp);
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
                        entryImages.Images.Add(new EntryImage
                        {
                            Url = _appHelper.GetImagePath(item.Url, ""),
                            Modifier = item.Modifier,
                            Note = item.Note
                        });
                    }
                }
            }

            //判断审核是否为空
            if (entryImages.Images.Count == 0)
            {
                return new Result { Successful = true };
            }
            //检查图片链接 是否包含外链
            foreach (var item in entryImages.Images)
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
                serializer.Serialize(text, entryImages);
                resulte = text.ToString();
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishImagesAsync(entry, entryImages);
                await _examineService.UniversalEditExaminedAsync(entry, user, true, resulte, Operation.EstablishImages, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishImages);

            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(entry, user, false, resulte, Operation.EstablishImages, model.Note);
            }
            return new Result { Successful = true };

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditRelevancesViewModel>> EditRelevances(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.Relevances).FirstOrDefaultAsync(s => s.Id == Id && s.IsHidden != true);
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
            foreach (var item in entry.Relevances)
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
                    case "动态":
                        news.Add(new RelevancesModel
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
            var entry = await _entryRepository.GetAll()
              .Include(s => s.Relevances)
              .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }
            //处理原始数据 删除空项目
            if (model.Roles != null)
            {
                for (var i = 0; i < model.Roles.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(model.Roles[i].DisplayName))
                    {
                        model.Roles.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (model.staffs != null)
            {
                for (var i = 0; i < model.staffs.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(model.staffs[i].DisplayName))
                    {
                        model.staffs.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (model.articles != null)
            {
                for (var i = 0; i < model.articles.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(model.articles[i].DisplayName))
                    {
                        model.articles.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (model.news != null)
            {
                for (var i = 0; i < model.news.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(model.news[i].DisplayName))
                    {
                        model.news.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (model.others != null)
            {
                for (var i = 0; i < model.others.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(model.others[i].DisplayName))
                    {
                        model.others.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (model.Groups != null)
            {
                for (var i = 0; i < model.Groups.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(model.Groups[i].DisplayName))
                    {
                        model.Groups.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (model.Games != null)
            {
                for (var i = 0; i < model.Games.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(model.Games[i].DisplayName))
                    {
                        model.Games.RemoveAt(i);
                        i--;
                    }
                }
            }

            //创建审核数据模型
            var examinedModel = new EntryRelevancesModel
            {
                Relevances = new List<EntryRelevancesExaminedModel>()
            };
            //遍历当前词条数据 打上删除标签
            foreach (var item in entry.Relevances)
            {
                examinedModel.Relevances.Add(new EntryRelevancesExaminedModel
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
                    var listTemp = entry.Relevances.Where(s => s.Modifier == "角色");
                    foreach (var infor in listTemp)
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
                        examinedModel.Relevances.Add(new EntryRelevancesExaminedModel
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
                    var listTemp = entry.Relevances.Where(s => s.Modifier == "STAFF");
                    foreach (var infor in listTemp)
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
                        examinedModel.Relevances.Add(new EntryRelevancesExaminedModel
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
                    var listTemp = entry.Relevances.Where(s => s.Modifier == "文章");
                    foreach (var infor in listTemp)
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
                        examinedModel.Relevances.Add(new EntryRelevancesExaminedModel
                        {
                            DisplayName = item.DisplayName,
                            Modifier = "文章",
                            IsDelete = false
                        });
                    }
                }
            }
            if (model.news != null)
            {
                //循环查找是否相同
                foreach (var item in model.news)
                {
                    var isSame = false;
                    var listTemp = entry.Relevances.Where(s => s.Modifier == "动态");
                    foreach (var infor in listTemp)
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
                        examinedModel.Relevances.Add(new EntryRelevancesExaminedModel
                        {
                            DisplayName = item.DisplayName,
                            Modifier = "动态",
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
                    var listTemp = entry.Relevances.Where(s => s.Modifier == "制作组");
                    foreach (var infor in listTemp)
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
                        examinedModel.Relevances.Add(new EntryRelevancesExaminedModel
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
                    var listTemp = entry.Relevances.Where(s => s.Modifier == "游戏");
                    foreach (var infor in listTemp)
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
                        examinedModel.Relevances.Add(new EntryRelevancesExaminedModel
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
                    var isSame = false;
                    var listTemp = entry.Relevances.Where(s => s.Modifier == "其他");
                    foreach (var infor in listTemp)
                    {
                        if (item.DisplayName == infor.DisplayName && item.DisPlayValue == infor.DisplayValue && item.Link == infor.Link)
                        {
                            //如果两次一致 删除上一步中的项目
                            foreach (var temp in examinedModel.Relevances)
                            {
                                if (temp.DisplayName == infor.DisplayName && temp.DisplayValue == infor.DisplayValue && temp.Link == infor.Link)
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
                        examinedModel.Relevances.Add(new EntryRelevancesExaminedModel
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
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishRelevancesAsync(entry, examinedModel);
                await _examineService.UniversalEditExaminedAsync(entry, user, true, resulte, Operation.EstablishRelevances, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishRelevances);
            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(entry, user, false, resulte, Operation.EstablishRelevances, model.Note);
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
            var entry = await _entryRepository.GetAll()
              .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (entry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }

            //判断是否一致
            if (model.Context == entry.MainPage)
            {
                return new Result { Successful = true };
            }
            if (model.Context == null)
            {
                model.Context = "";
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishMainPageAsync(entry, model.Context);
                await _examineService.UniversalEditExaminedAsync(entry, user, true, model.Context, Operation.EstablishMainPage, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishMainPage);
            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(entry, user, false, model.Context, Operation.EstablishMainPage, model.Note);
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
                var tagIds = new List<int>();

                var tagNames = new List<string>();
                tagNames.AddRange(model.Tags.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

                foreach (var item in tagNames)
                {
                    var infor = await _tagRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                    if (infor <= 0)
                    {
                        return new Result { Successful = false, Error = "标签 " + item + " 不存在" };
                    }
                    else
                    {
                        tagIds.Add(infor);
                    }
                }
                //删除重复数据
                tagIds = tagIds.Distinct().ToList();

                //第一步 建立词条主要信息

                //添加修改记录
                //新建审核数据对象
                var entryMain = new EntryMain
                {
                    Name = model.Name,
                    BriefIntroduction = model.BriefIntroduction,
                    MainPicture = model.MainPicture,
                    Thumbnail = model.Thumbnail,
                    BackgroundPicture = model.BackgroundPicture,
                    Type = model.Type,
                    DisplayName = model.DisplayName,
                    SmallBackgroundPicture = model.SmallBackgroundPicture,
                    AnotherName = model.AnotherName
                };
                //序列化
                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, entryMain);
                    resulte = text.ToString();
                }
                //将空词条添加到数据库中 目的是为了获取索引
                var entry = new Entry
                {
                    Type = model.Type,
                    LastEditTime = DateTime.Now.ToCstTime()
                };
                entry = await _entryRepository.InsertAsync(entry);
                //初始化列表
                entry.Information = new List<BasicEntryInformation>();
                entry.Relevances = new List<EntryRelevance>();
                entry.Pictures = new List<EntryPicture>();

                //判断是否是管理员
                if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                {
                    await _examineService.ExamineEstablishMainAsync(entry, entryMain);
                    await _examineService.UniversalEstablishExaminedAsync(entry, user, true, resulte, Operation.EstablishMain, model.Note);
                    await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishMain);

                }
                else
                {
                    await _examineService.UniversalEstablishExaminedAsync(entry, user, false, resulte, Operation.EstablishMain, model.Note);
                }

                //第二步 建立词条附加信息
                //根据类别进行序列化操作
                var basics = new List<BasicEntryInformation>();
                switch (model.Type)
                {
                    case EntryType.Game:
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
                        if (model.InforStaffs != null)
                        {
                            //序列化STAFF
                            foreach (var item in model.InforStaffs)
                            {

                                var staffs = new List<BasicEntryInformationAdditional>
                                {
                                    new BasicEntryInformationAdditional { DisplayName = "职位（官方称呼）", DisplayValue = item.PositionOfficial },
                                    new BasicEntryInformationAdditional { DisplayName = "昵称（官方称呼）", DisplayValue = item.NicknameOfficial },
                                    new BasicEntryInformationAdditional { DisplayName = "职位（通用）", DisplayValue = item.PositionGeneral.ToString() },
                                    new BasicEntryInformationAdditional { DisplayName = "角色", DisplayValue = item.Role },
                                    new BasicEntryInformationAdditional { DisplayName = "子项目", DisplayValue = item.Subcategory },
                                    new BasicEntryInformationAdditional { DisplayName = "隶属组织", DisplayValue = item.SubordinateOrganization }
                                };
                                basics.Add(new BasicEntryInformation { Modifier = "STAFF", DisplayName = item.PositionOfficial, DisplayValue = item.NicknameOfficial, Additional = staffs });

                            }
                        }


                        //添加基本信息
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间", DisplayValue = model.IssueTime?.ToString("yyyy年M月d日") });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间备注", DisplayValue = model.IssueTimeString });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "原作", DisplayValue = model.Original });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "制作组", DisplayValue = model.ProductionGroup });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "游戏平台", DisplayValue = gamePlatforms });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "引擎", DisplayValue = model.Engine });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行商", DisplayValue = model.Publisher });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行方式", DisplayValue = model.IssueMethod });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "官网", DisplayValue = model.OfficialWebsite });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "Steam平台Id", DisplayValue = model.SteamId });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGame });
                        break;
                    case EntryType.ProductionGroup:
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGroup });
                        break;
                    case EntryType.Role:
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "声优", DisplayValue = model.CV });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性别", DisplayValue = model.Gender.ToString() });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材数据", DisplayValue = model.FigureData });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材(主观)", DisplayValue = model.FigureSubjective });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "生日", DisplayValue = model.Birthday?.ToString("M月d日") });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发色", DisplayValue = model.Haircolor });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "瞳色", DisplayValue = model.Pupilcolor });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性格", DisplayValue = model.Character });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "角色身份", DisplayValue = model.RoleIdentity });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "血型", DisplayValue = model.BloodType });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身高", DisplayValue = model.RoleHeight });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "兴趣", DisplayValue = model.RoleTaste });
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "年龄", DisplayValue = model.RoleAge });
                        break;
                    case EntryType.Staff:
                        basics.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "昵称（官方称呼）", DisplayValue = model.Nickname });
                        break;
                }


                //新建审核数据对象
                var entryAddInfor = new EntryAddInfor
                {
                    Information = new List<BasicEntryInformation_>()
                };
                //添加修改记录
                foreach (var item in basics)
                {
                    List<BasicEntryInformationAdditional_> addInfors = null;
                    if (item.Additional != null)
                    {
                        addInfors = new List<BasicEntryInformationAdditional_>();
                        foreach (var item_1 in item.Additional)
                        {
                            addInfors.Add(new BasicEntryInformationAdditional_
                            {
                                DisplayName = item_1.DisplayName,
                                DisplayValue = item_1.DisplayValue,
                                IsDelete = false
                            });
                        }

                    }
                    entryAddInfor.Information.Add(new BasicEntryInformation_
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue,
                        IsDelete = false,
                        Additional = addInfors,
                        Modifier = item.Modifier
                    });
                }
                //序列化相关网站
                if (model.SocialPlatforms != null)
                {
                    //遍历一遍当前视图中 相关网站
                    foreach (var item in model.SocialPlatforms)
                    {
                        entryAddInfor.Information.Add(new BasicEntryInformation_ { Modifier = "相关网站", DisplayName = item.Name, DisplayValue = item.Link, IsDelete = false });
                    }
                }
                //如果 DisplayValue 为null 则不添加审核记录
                for (var i = 0; i < entryAddInfor.Information.Count; i++)
                {
                    if (entryAddInfor.Information[i].IsDelete == false && entryAddInfor.Information[i].DisplayValue == null)
                    {
                        entryAddInfor.Information.RemoveAt(i);
                        i--;
                    }
                }
                //判断审核是否为空
                if (entryAddInfor.Information.Count != 0)
                { //序列化
                    resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, entryAddInfor);
                        resulte = text.ToString();
                    }
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                    {
                        await _examineService.ExamineEstablishAddInforAsync(entry, entryAddInfor);
                        await _examineService.UniversalEstablishExaminedAsync(entry, user, true, resulte, Operation.EstablishAddInfor, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishAddInfor);

                    }
                    else
                    {
                        await _examineService.UniversalEstablishExaminedAsync(entry, user, false, resulte, Operation.EstablishAddInfor, model.Note);
                    }
                }

                //第三步 建立词条图片

                //创建审核数据模型
                entry.Pictures = new List<EntryPicture>();

                var entryImages = new EntryImages
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
                            entryImages.Images.Add(new EntryImage
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
                if (entryImages.Images.Count != 0)
                {
                    //序列化JSON
                    resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, entryImages);
                        resulte = text.ToString();
                    }

                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                    {
                        await _examineService.ExamineEstablishImagesAsync(entry, entryImages);
                        await _examineService.UniversalEstablishExaminedAsync(entry, user, true, resulte, Operation.EstablishImages, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishImages);

                    }
                    else
                    {
                        await _examineService.UniversalEstablishExaminedAsync(entry, user, false, resulte, Operation.EstablishImages, model.Note);
                    }
                }


                //第四步 建立词条关联信息

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
                if (model.ReStaffs != null)
                {
                    foreach (var item in model.ReStaffs)
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
                if (model.News != null)
                {
                    foreach (var item in model.News)
                    {
                        if (string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            entryRelevances.Add(new EntryRelevance
                            {
                                Modifier = "动态",
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
                if (model.Others != null)
                {
                    foreach (var item in model.Others)
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
                    var entryRelevancesModel = new EntryRelevancesModel
                    {
                        Relevances = new List<EntryRelevancesExaminedModel>()
                    };
                    foreach (var item in entryRelevances)
                    {
                        entryRelevancesModel.Relevances.Add(new EntryRelevancesExaminedModel
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
                        serializer.Serialize(text, entryRelevancesModel);
                        resulte = text.ToString();
                    }
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                    {
                        await _examineService.ExamineEstablishRelevancesAsync(entry, entryRelevancesModel);
                        await _examineService.UniversalEstablishExaminedAsync(entry, user, true, resulte, Operation.EstablishRelevances, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishRelevances);

                    }
                    else
                    {
                        await _examineService.UniversalEstablishExaminedAsync(entry, user, false, resulte, Operation.EstablishRelevances, model.Note);
                    }
                }

                //第五步 建立词条主页
                //判断是否为空
                if (string.IsNullOrWhiteSpace(model.Context) == false)
                {
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                    {
                        await _examineService.ExamineEstablishMainPageAsync(entry, model.Context);
                        await _examineService.UniversalEstablishExaminedAsync(entry, user, true, model.Context, Operation.EstablishMainPage, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishMainPage);

                    }
                    else
                    {
                        await _examineService.UniversalEstablishExaminedAsync(entry, user, false, model.Context, Operation.EstablishMainPage, model.Note);
                    }
                }

                //第六步 建立词条标签
                //创建审核数据模型
                var examinedModel = new EntryTags();

                //添加新建项目
                foreach (var item in tagIds)
                {
                    examinedModel.Tags.Add(new EntryTagsAloneModel
                    {
                        TagId = item,
                        IsDelete = false
                    });

                }

                //判断审核是否为空
                if (examinedModel.Tags.Count != 0)
                {
                    //序列化JSON
                    resulte = "";
                    using (TextWriter text = new StringWriter())
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(text, examinedModel);
                        resulte = text.ToString();
                    }
                    //判断是否是管理员
                    if (await _userManager.IsInRoleAsync(user, "Editor") == true)
                    {
                        await _examineService.ExamineEstablishTagsAsync(entry, examinedModel);
                        await _examineService.UniversalEditExaminedAsync(entry, user, true, resulte, Operation.EstablishTags, model.Note);
                        await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishTags);
                    }
                    else
                    {
                        await _examineService.UniversalEditExaminedAsync(entry, user, false, resulte, Operation.EstablishTags, model.Note);
                    }

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
            var entry = await _entryRepository.GetAll()
                  .Include(s => s.Tags)
                  .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entry == null)
            {
                return new Result { Error = $"无法找到ID为{model.Id}的词条", Successful = false };
            }

            //预处理 建立词条关联信息
            //判断关联是否存在
            var tagIds = new List<int>();

            var tagNames = new List<string>();
            tagNames.AddRange(model.Tags.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false).Select(s => s.DisplayName));

            foreach (var item in tagNames)
            {
                var infor = await _tagRepository.GetAll().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    return new Result { Successful = false, Error = "标签 " + item + " 不存在" };
                }
                else
                {
                    tagIds.Add(infor);
                }
            }
            //删除重复数据
            tagIds = tagIds.Distinct().ToList();


            //创建审核数据模型
            var examinedModel = new EntryTags();

            //遍历当前词条数据 打上删除标签
            foreach (var item in entry.Tags)
            {
                examinedModel.Tags.Add(new EntryTagsAloneModel
                {
                    TagId = item.Id,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in tagIds.Where(s => examinedModel.Tags.Select(s => s.TagId).Contains(s) == false))
            {
                examinedModel.Tags.Add(new EntryTagsAloneModel
                {
                    TagId = item,
                    IsDelete = false
                });

            }
            //删除不存在的老项目
            examinedModel.Tags.RemoveAll(s => tagIds.Contains(s.TagId) && s.IsDelete);

            //判断审核是否为空
            if (examinedModel.Tags.Count == 0)
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
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEstablishTagsAsync(entry, examinedModel);
                await _examineService.UniversalEditExaminedAsync(entry, user, true, resulte, Operation.EstablishTags, model.Note);
                await _appHelper.AddUserContributionValueAsync(user.Id, entry.Id, Operation.EstablishTags);
            }
            else
            {
                await _examineService.UniversalEditExaminedAsync(entry, user, false, resulte, Operation.EstablishTags, model.Note);
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
            return await _entryRepository.GetAll().AsNoTracking().Where(s =>  s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => new NameIdPairModel { Name = s.Name, Id = s.Id }).ToArrayAsync();
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
    }
}
