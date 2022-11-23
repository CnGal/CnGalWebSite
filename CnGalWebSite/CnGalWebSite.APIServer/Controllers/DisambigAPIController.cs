using CnGalWebSite.APIServer.Application.Disambigs;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel.Dismbigs;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Disambig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/disambigs/[action]")]

    public class DisambigAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Disambig, int> _disambigRepository;

        private readonly IAppHelper _appHelper;
        private readonly IDisambigService _disambigService;
        private readonly IExamineService _examineService;
        private readonly IEditRecordService _editRecordService;

        public DisambigAPIController(IRepository<Disambig, int> disambigRepository, IDisambigService disambigService, IEditRecordService editRecordService,
        UserManager<ApplicationUser> userManager, IExamineService examineService, IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository,
       IRepository<Examine, long> examineRepository)
        {
            _userManager = userManager;
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _disambigRepository = disambigRepository;
            _disambigService = disambigService;
            _examineService = examineService;
            _editRecordService = editRecordService;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<DisambigViewModel>> GetDisambigViewAsync(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取文章
            var disambig = await _disambigRepository.GetAll().AsNoTracking().Include(s => s.Articles).Include(s => s.Entries).Include(s => s.Examines).FirstOrDefaultAsync(x => x.Id == id);
            if (disambig == null)
            {
                return NotFound();
            }

            List<Examine> examineQuery = null;
            //读取审核信息
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll()
                               .Where(s => s.DisambigId == disambig.Id && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.DisambigMain || s.Operation == Operation.DisambigRelevances))
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
                examine = examineQuery.Find(s => s.Operation == Operation.DisambigMain);
                if (examine != null)
                {
                    await _disambigService.UpdateDisambigDataAsync(disambig, examine);
                }
            }

            //建立视图模型
            var model = new DisambigViewModel
            {
                Id = disambig.Id,
                Name = disambig.Name,
                BriefIntroduction = disambig.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(disambig.MainPicture, ""),
                BackgroundPicture = _appHelper.GetImagePath(disambig.BackgroundPicture, ""),
                SmallBackgroundPicture = _appHelper.GetImagePath(disambig.SmallBackgroundPicture, ""),

            };

            //判断当前是否隐藏
            if (disambig.IsHidden == true)
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
                examine = examineQuery.Find(s => s.Operation == Operation.DisambigMain);
                if (examine != null)
                {
                    model.MainState = EditState.Preview;
                }
            }

            //序列化相关性列表
            //再读取当前用户等待审核的信息
            if (user != null)
            {
                examine = examineQuery.Find(s => s.Operation == Operation.DisambigRelevances);
                if (examine != null)
                {
                    model.RelevancesState = EditState.Preview;
                    await _disambigService.UpdateDisambigDataAsync(disambig, examine);
                }
            }
            //先读取词条信息
            var relevances = new List<DisambigAloneModel>();
            foreach (var item in disambig.Entries)
            {
                relevances.Add(new DisambigAloneModel
                {
                    entry = _appHelper.GetEntryInforTipViewModel(item)
                });
            }
            foreach (var item in disambig.Articles)
            {
                relevances.Add(new DisambigAloneModel
                {
                    article = _appHelper.GetArticleInforTipViewModel(item)
                });
            }


            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.DisambigId == disambig.Id && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }
            //获取各部分状态
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.DisambigMain))
                    {
                        model.MainState = EditState.Locked;
                    }
                    else
                    {
                        model.MainState = EditState.Normal;
                    }
                }
                if (model.RelevancesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.DisambigRelevances))
                    {
                        model.RelevancesState = EditState.Locked;
                    }
                    else
                    {
                        model.RelevancesState = EditState.Normal;
                    }
                }
            }

            //赋值
            model.Relecances = relevances;

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> CreateDisambigAsync(CreateDisambigViewModel model)
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
                if (await _disambigRepository.FirstOrDefaultAsync(s => s.Name == model.Name) != null)
                {
                    return new Result { Error = "该消歧义页面的名称与其他消歧义页面重复", Successful = false };
                }
                //第一步 处理主要信息

                //新建审核数据对象
                var disambigMain = new DisambigMain
                {
                    Name = model.Name,
                    BriefIntroduction = model.BriefIntroduction,
                    MainPicture = model.MainPicture,
                    BackgroundPicture = model.BackgroundPicture,
                    SmallBackgroundPicture = model.SmallBackgroundPicture,
                };
                //序列化
                var resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(text, disambigMain);
                    resulte = text.ToString();
                }
                //将空文章添加到数据库中 目的是为了获取索引
                var disambig = new Disambig();

                disambig = await _disambigRepository.InsertAsync(disambig);

                //保存并尝试应用审核记录
                await _editRecordService.SaveAndApplyEditRecord(disambig, user, disambigMain, Operation.DisambigMain, model.Note, true);


                //第二步 处理关联词条

                //转化为标准词条相关性列表格式
                var relevances = new List<DisambigRelevancesExaminedModel>();

                foreach (var item in model.Entries)
                {
                    var index = await _entryRepository.GetAll().Where(s => s.Name == item.Name).Select(s => s.Id).FirstOrDefaultAsync();
                    if (index != 0)
                    {
                        relevances.Add(new DisambigRelevancesExaminedModel { IsDelete = false, EntryId = index, Type = DisambigRelevanceType.Entry });
                    }
                }
                foreach (var item in model.Articles)
                {
                    var index = await _articleRepository.GetAll().Where(s => s.Name == item.Name).Select(s => s.Id).FirstOrDefaultAsync();
                    if (index != 0)
                    {
                        relevances.Add(new DisambigRelevancesExaminedModel { IsDelete = false, EntryId = index, Type = DisambigRelevanceType.Article });
                    }
                }

                //判断审核是否为空
                if (relevances.Count != 0)
                {
                    //创建审核数据模型
                    var disambigRelevances = new DisambigRelevances
                    {
                        Relevances = relevances
                    };

                    //保存并尝试应用审核记录
                    await _editRecordService.SaveAndApplyEditRecord(disambig, user, disambigRelevances, Operation.DisambigRelevances, model.Note,true);

                }
                return new Result { Successful = true, Error = disambig.Id.ToString() };
            }
            catch
            {
                return new Result { Error = "创建消歧义页的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditDisambigViewModel>> EditDisambigAsync(int Id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取词条
            var disambig = await _disambigRepository.GetAll().AsNoTracking().Include(s => s.Entries).Include(s => s.Articles).FirstOrDefaultAsync(s => s.Id == Id);
            if (disambig == null)
            {
                return NotFound();
            }

            //判断是否为锁定状态
            if (await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.ApplicationUserId != user.Id && s.IsPassed == null && (s.Operation == Operation.DisambigMain || s.Operation == Operation.DisambigRelevances)))
            {
                return NotFound();
            }

            var model = new EditDisambigViewModel();
            //获取审核记录

            //第一步 获取主要信息
            var examine = await _examineRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null && s.Operation == Operation.DisambigMain);
            if (examine != null)
            {
                await _disambigService.UpdateDisambigDataAsync(disambig, examine);
            }

            model.MainPicturePath = _appHelper.GetImagePath(disambig.MainPicture, "app.png");
            model.BackgroundPicturePath = _appHelper.GetImagePath(disambig.BackgroundPicture, "app.png");
            model.MainPicture = disambig.MainPicture;
            model.BackgroundPicture = disambig.BackgroundPicture;

            model.Name = disambig.Name;
            model.BriefIntroduction = disambig.BriefIntroduction;

            model.SmallBackgroundPicture = disambig.SmallBackgroundPicture;
            model.SmallBackgroundPicturePath = _appHelper.GetImagePath(disambig.SmallBackgroundPicture, "app.png");
            model.Id = Id;

            //第二步 获取关联词条
            examine = await _examineRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null && s.Operation == Operation.DisambigRelevances);
            if (examine != null)
            {
                await _disambigService.UpdateDisambigDataAsync(disambig, examine);
            }

            var entries = new List<DisambigRelevanceModel>();
            foreach (var item in disambig.Entries)
            {
                entries.Add(new DisambigRelevanceModel { Name = item.Name });
            }
            var articles = new List<DisambigRelevanceModel>();
            foreach (var item in disambig.Articles)
            {
                articles.Add(new DisambigRelevanceModel { Name = item.Name });
            }

            model.Entries = entries;
            model.Articles = articles;

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditDisambigAsync(EditDisambigViewModel model)
        {
            try
            {
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                //检查是否超过编辑上限
                if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
                {
                    return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
                }

                //判断是否为锁定状态
                if (await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId != user.Id && s.DisambigId == model.Id && (s.Operation == Operation.DisambigMain || s.Operation == Operation.DisambigRelevances)))
                {
                    return new Result { Error = "当前消歧义页已经被另一名用户编辑，正在等待审核,请等待审核结束后再进行编辑", Successful = false };
                }

                //查找当前文章
                var disambig = await _disambigRepository.GetAll().Include(s => s.Entries).Include(s => s.Articles).FirstOrDefaultAsync(s => s.Id == model.Id);
                if (disambig == null)
                {
                    return new Result { Error = $"无法找到ID为{model.Id}的消歧义页", Successful = false };
                }
                //判断名称是否重复
                if (model.Name != disambig.Name && await _disambigRepository.GetAll().AnyAsync(s => s.Name == model.Name && s.Id != model.Id) == true)
                {
                    return new Result { Error = "该消歧义页的名称与其他消歧义页重复", Successful = false };
                }

                //第一步 修改文章主要信息

                //判断是否修改
                if (disambig.SmallBackgroundPicture != model.SmallBackgroundPicture || disambig.Name != model.Name ||
                    disambig.BriefIntroduction != model.BriefIntroduction || disambig.MainPicture != model.MainPicture
                    || disambig.BackgroundPicture != model.BackgroundPicture)
                {
                    //添加修改记录
                    //新建审核数据对象
                    var disambigMain = new DisambigMain
                    {
                        Name = model.Name,
                        BriefIntroduction = model.BriefIntroduction,
                        MainPicture = model.MainPicture,
                        BackgroundPicture = model.BackgroundPicture,
                        SmallBackgroundPicture = model.SmallBackgroundPicture
                    };

                    //保存并尝试应用审核记录
                    await _editRecordService.SaveAndApplyEditRecord(disambig, user, disambigMain, Operation.DisambigMain, model.Note);

                }


                //第二步 修改文章关联词条

                //删除空数据
                for (var i = 0; i < model.Entries.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(model.Entries[i].Name))
                    {
                        model.Entries.RemoveAt(i);
                        i--;
                    }
                }
                for (var i = 0; i < model.Articles.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(model.Articles[i].Name))
                    {
                        model.Articles.RemoveAt(i);
                        i--;
                    }
                }

                //创建审核数据模型
                var disambigRelevances = new DisambigRelevances
                {
                    Relevances = new List<DisambigRelevancesExaminedModel>()
                };
                //遍历当前词条数据 打上删除标签
                foreach (var item in disambig.Entries)
                {
                    disambigRelevances.Relevances.Add(new DisambigRelevancesExaminedModel
                    {
                        EntryId = item.Id,
                        Type = DisambigRelevanceType.Entry,
                        IsDelete = true,
                    });
                }
                foreach (var item in disambig.Articles)
                {
                    disambigRelevances.Relevances.Add(new DisambigRelevancesExaminedModel
                    {
                        EntryId = item.Id,
                        Type = DisambigRelevanceType.Article,
                        IsDelete = true,
                    });
                }

                //再遍历视图 对应修改
                if (model.Entries != null)
                {
                    //循环查找词条是否相同
                    foreach (var item in model.Entries)
                    {
                        var temp = await _entryRepository.GetAll().Where(s => s.Name == item.Name).Select(s => s.Id).FirstOrDefaultAsync();
                        if (temp <= 0)
                        {
                            return new Result { Successful = false, Error = "词条『" + item.Name + "』不存在" };
                        }

                        var isSame = false;
                        foreach (var infor in disambig.Entries)
                        {
                            //查找词条
                            if (temp == infor.Id)
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp_ in disambigRelevances.Relevances)
                                {
                                    if (temp_.EntryId == infor.Id && temp_.Type == DisambigRelevanceType.Entry)
                                    {
                                        disambigRelevances.Relevances.Remove(temp_);
                                        isSame = true;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if (isSame == false && string.IsNullOrWhiteSpace(item.Name) == false)
                        {
                            disambigRelevances.Relevances.Add(new DisambigRelevancesExaminedModel
                            {
                                EntryId = temp,
                                Type = DisambigRelevanceType.Entry,
                                IsDelete = false
                            });
                        }
                    }
                    //循环查找文章是否相同
                    foreach (var item in model.Articles)
                    {
                        var temp = await _articleRepository.GetAll().Where(s => s.Name == item.Name).Select(s => s.Id).FirstOrDefaultAsync();
                        if (temp <= 0)
                        {
                            return new Result { Successful = false, Error = "文章『" + item.Name + "』不存在" };
                        }

                        var isSame = false;
                        foreach (var infor in disambig.Articles)
                        {
                            //查找文章
                            if (temp == infor.Id)
                            {
                                //如果两次一致 删除上一步中的项目
                                foreach (var temp_ in disambigRelevances.Relevances)
                                {
                                    if (temp_.EntryId == infor.Id && temp_.Type == DisambigRelevanceType.Article)
                                    {
                                        disambigRelevances.Relevances.Remove(temp_);
                                        isSame = true;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if (isSame == false && string.IsNullOrWhiteSpace(item.Name) == false)
                        {
                            disambigRelevances.Relevances.Add(new DisambigRelevancesExaminedModel
                            {
                                EntryId = temp,
                                Type = DisambigRelevanceType.Article,
                                IsDelete = false
                            });
                        }
                    }
                }

                //判断审核是否为空
                if (disambigRelevances.Relevances.Count != 0)
                {
                    //保存并尝试应用审核记录
                    await _editRecordService.SaveAndApplyEditRecord(disambig, user, disambigRelevances, Operation.DisambigRelevances, model.Note);

                }
                return new Result { Successful = true, Error = disambig.Id.ToString() };
            }
            catch
            {
                return new Result { Error = "修改消歧义页的过程中发生未知错误，请确保数据格式正确后联系管理员", Successful = false };

            }

        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListDisambigAloneModel>>> GetDisambigListNormalAsync(DisambigsPagesInfor input)
        {
            var dtos = await _disambigService.GetPaginatedResult(input.Options, input.SearchModel);
            return dtos;
        }

    }
}
