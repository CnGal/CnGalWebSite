using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Home;
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
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Users;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/examines/[action]")]
    [ApiController]
    public class ExaminesAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Disambig, int> _disambigRepository;
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<UserMonitor, long> _userMonitorsRepository;
        private readonly IRepository<UserReviewEditRecord, long> _userReviewEditRecordRepository;
        private readonly IRepository<UserCertification, long> _userCertificationRepository;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;
        private readonly IRankService _rankService;
        private readonly IEditRecordService _editRecordService;
        private readonly IUserService _userService;


        public ExaminesAPIController(IRepository<Disambig, int> disambigRepository, IRankService rankService, IRepository<Comment, long> commentRepository, IUserService userService,
        IRepository<Message, long> messageRepository, IRepository<ApplicationUser, string> userRepository, IRepository<UserReviewEditRecord, long> userReviewEditRecordRepository,
        UserManager<ApplicationUser> userManager, IExamineService examineService, IRepository<UserMonitor, long> userMonitorsRepository, IEditRecordService editRecordService, IRepository<UserCertification, long> userCertificationRepository,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Periphery, long> peripheryRepository, IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IRepository<PlayedGame, long> playedGameRepository)
        {
            _userManager = userManager;
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _tagRepository = tagRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _examineService = examineService;
            _messageRepository = messageRepository;
            _commentRepository = commentRepository;
            _disambigRepository = disambigRepository;
            _rankService = rankService;
            _peripheryRepository = peripheryRepository;
            _playedGameRepository = playedGameRepository;
            _userRepository = userRepository;
            _userMonitorsRepository = userMonitorsRepository;
            _userReviewEditRecordRepository = userReviewEditRecordRepository;
            _editRecordService = editRecordService;
            _userCertificationRepository = userCertificationRepository;
            _userService = userService;
        }



        /// <summary>
        /// 获取编辑记录详细信息视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [HttpGet]
        public async Task<ActionResult<ExamineViewModel>> GetExamineViewAsync(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取审核单
            Examine examine =
                examine = await _examineRepository.GetAll()
                        .Include(s => s.ApplicationUser)
                        .FirstOrDefaultAsync(x => x.Id == id);


            if (examine == null)
            {
                return NotFound();
            }
            //对应赋值
            var model = new ExamineViewModel
            {
                Id = examine.Id,
                UserId = examine.ApplicationUserId,
                ApplyTime = examine.ApplyTime,
                Operation = examine.Operation,
                UserName = examine.ApplicationUser.UserName,
                Comments = examine.Comments,
                PassedAdminName = examine.PassedAdminName,
                Note = examine.Note,
                PassedTime = examine.PassedTime,
                IsPassed = examine.IsPassed,
            };
            //判断是否有前置审核
            if (examine.PrepositionExamineId != null)
            {
                var temp = await _examineRepository.FirstOrDefaultAsync(s => s.Id == examine.PrepositionExamineId && s.IsPassed == null);
                if (temp == null)
                {
                    model.PrepositionExamineId = -1;
                }
                else
                {
                    model.PrepositionExamineId = (int)examine.PrepositionExamineId;
                }
            }


            model.IsAdmin = await _editRecordService.CheckUserExaminePermission(examine, user);

            if (await _examineService.GetExamineView(model, examine) == false)
            {
                return NotFound();
            }

            //获取敏感词列表
            //if (string.IsNullOrWhiteSpace(examine.Context) == false && user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            //{
            //    model.SensitiveWords = await _appHelper.GetSensitiveWordsInText(examine.Context);
            //}
            return model;
        }

        /// <summary>
        /// 对编辑进行审核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<Result>> ProcAsync(Models.ExaminedViewModel model)
        {
            var examine = await _examineRepository.GetAll().Include(s => s.ApplicationUser).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (examine == null)
            {
                return NotFound();
            }

            //判断是否已审核
            if (examine.IsPassed != null)
            {
                return new Result { Successful = false, Error = "该记录已经被被审核，不能修改审核状态" };
            }

            //判断是否有前置审核
            if (examine.PrepositionExamineId != null)
            {
                var temp = await _examineRepository.FirstOrDefaultAsync(s => s.Id == examine.PrepositionExamineId && s.IsPassed == null);
                if (temp != null)
                {
                    return new Result { Successful = false, Error = $"该审核有一个前置审核,请先对前置审核进行审核，ID{examine.PrepositionExamineId}" };
                }
            }

            //获取当前用户
            var currentUser = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取目标对象
            object entry = null;

            //根据操作类别进行操作
            switch (examine.Operation)
            {
                case Operation.UserMainPage:
                    entry = await _userRepository.GetAll().FirstOrDefaultAsync(s => s.Id == examine.ApplicationUserId);
                    break;
                case Operation.EditUserMain:
                    entry = await _userRepository.GetAll().FirstOrDefaultAsync(s => s.Id == examine.ApplicationUserId);
                    break;
                case Operation.EstablishMain:
                    entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);

                    break;
                case Operation.EstablishAddInfor:

                    entry = await _entryRepository.GetAll()
                                               .Include(s => s.Information).ThenInclude(s => s.Additional)
                                               .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                                               .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                    break;
                case Operation.EstablishImages:
                    entry = await _entryRepository.GetAll()
                       .Include(s => s.Pictures)
                       .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                    break;
                case Operation.EstablishAudio:
                    entry = await _entryRepository.GetAll()
                       .Include(s => s.Audio)
                       .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                    break;
                case Operation.EstablishRelevances:
                    entry = await _entryRepository.GetAll()
                        .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                        .Include(s => s.Outlinks)
                        .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                    break;
                case Operation.EstablishTags:
                    entry = await _entryRepository.GetAll().Include(s => s.Tags).FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                    break;
                case Operation.EstablishMainPage:
                    entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                    break;
                case Operation.EditArticleMain:

                    entry = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);

                    break;
                case Operation.EditArticleRelevanes:
                    entry = await _articleRepository.GetAll()
                            .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                            .FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                    break;
                case Operation.EditArticleMainPage:
                    entry = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                    break;
                case Operation.EditTagMain:
                    entry = await _tagRepository.GetAll().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == examine.TagId);
                    break;
                case Operation.EditTagChildTags:
                    entry = await _tagRepository.GetAll().Include(s => s.InverseParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == examine.TagId);

                    break;
                case Operation.EditTagChildEntries:
                    entry = await _tagRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == examine.TagId);
                    break;
                case Operation.PubulishComment:
                    entry = await _commentRepository.GetAll()
                            .Include(s => s.ParentCodeNavigation)
                            .FirstOrDefaultAsync(s => s.Id == examine.CommentId);
                    break;
                case Operation.DisambigMain:
                    entry = await _disambigRepository.FirstOrDefaultAsync(s => s.Id == examine.DisambigId);
                    break;
                case Operation.DisambigRelevances:
                    entry = await _disambigRepository.FirstOrDefaultAsync(s => s.Id == examine.DisambigId);
                    break;
                case Operation.EditPeripheryMain:
                    entry = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
                    break;
                case Operation.EditPeripheryImages:
                    entry = await _peripheryRepository.GetAll().Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
                    break;
                case Operation.EditPeripheryRelatedEntries:
                    entry = await _peripheryRepository.GetAll()
                            .Include(s => s.RelatedEntries)
                            .FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
                    break;
                case Operation.EditPeripheryRelatedPeripheries:
                    entry = await _peripheryRepository.GetAll()
                            .Include(s => s.PeripheryRelationFromPeripheryNavigation).ThenInclude(s => s.ToPeripheryNavigation)
                            .FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
                    break;
                case Operation.EditPlayedGameMain:
                    entry = await _playedGameRepository.GetAll()
                            .FirstOrDefaultAsync(s => s.Id == examine.PlayedGameId);
                    break;
                case Operation.RequestUserCertification:
                    entry = await _userCertificationRepository.GetAll()
                        .Include(s => s.Entry)
                        .Include(s => s.ApplicationUser)
                        .FirstOrDefaultAsync(s => s.ApplicationUserId == examine.ApplicationUserId);
                    break;
                default:
                    return new Result { Successful = false, Error = "未知的操作" };
            }

            if (entry == null)
            {
                return new Result { Successful = false, Error = "获取目标对象失败" };
            }

            //判断是否有权限审核目标对象
            if (await _editRecordService.CheckUserExaminePermission(entry, currentUser, examine.Operation == Operation.RequestUserCertification ? false : true) == false)
            {
                return new Result { Successful = false, Error = "权限不足" };
            }




            //判断是否通过
            if (model.IsPassed == true)
            {
                //序列化审核数据
                object examineData = null;
                if (string.IsNullOrWhiteSpace(examine.Context))
                {
                    return new Result { Successful = false, Error = "审核内容为空" };
                }
                if (examine.Operation == Operation.UserMainPage || examine.Operation == Operation.EditArticleMainPage || examine.Operation == Operation.EstablishMainPage)
                {
                    examineData = examine.Context;
                }
                else
                {
                    using TextReader str = new StringReader(examine.Context);
                    var serializer = new JsonSerializer();
                    examineData = serializer.Deserialize(str, examine.Operation switch
                    {
                        Operation.EditUserMain => typeof(UserMain),
                        Operation.EstablishMain => typeof(ExamineMain),
                        Operation.EstablishAddInfor => typeof(EntryAddInfor),
                        Operation.EstablishImages => typeof(EntryImages),
                        Operation.EstablishAudio => typeof(EntryAudioExamineModel),
                        Operation.EstablishRelevances => typeof(EntryRelevances),
                        Operation.EstablishTags => typeof(EntryTags),
                        Operation.EditArticleMain => typeof(ExamineMain),
                        Operation.EditArticleRelevanes => typeof(ArticleRelevances),
                        Operation.EditTagMain => typeof(ExamineMain),
                        Operation.EditTagChildTags => typeof(TagChildTags),
                        Operation.EditTagChildEntries => typeof(TagChildEntries),
                        Operation.PubulishComment => typeof(CommentText),
                        Operation.DisambigMain => typeof(DisambigMain),
                        Operation.DisambigRelevances => typeof(DisambigRelevances),
                        Operation.EditPeripheryMain => typeof(ExamineMain),
                        Operation.EditPeripheryImages => typeof(PeripheryImages),
                        Operation.EditPeripheryRelatedEntries => typeof(PeripheryRelatedEntries),
                        Operation.EditPeripheryRelatedPeripheries => typeof(PeripheryRelatedPeripheries),
                        Operation.EditPlayedGameMain => typeof(PlayedGameMain),
                        Operation.RequestUserCertification => typeof(UserCertificationMain),
                        _ => throw new NotImplementedException("未知的操作")
                    });
                }

                if (examineData == null)
                {
                    return new Result { Successful = false, Error = "获取审核数据失败" };
                }
                //应用审核记录
                await _examineService.ApplyEditRecordToObject(entry, examineData, examine.Operation);


                //修改审核状态
                examine.Comments = model.Comments;
                examine.PassedAdminName = currentUser.UserName;
                examine.PassedTime = DateTime.Now.ToCstTime();
                examine.IsPassed = true;

                if (await _userManager.IsInRoleAsync(currentUser, "Admin"))
                {
                    examine.ContributionValue = model.ContributionValue;
                }

         

                examine = await _examineRepository.UpdateAsync(examine);

                //尝试添加审阅
                await _editRecordService.AddEditRecordToUserReview(examine);

                //更新用户积分
                await _userService.UpdateUserIntegral(examine.ApplicationUser);
                await _rankService.UpdateUserRanks(examine.ApplicationUser);

            }
            else
            {
                //修改审核状态
                examine.Comments = model.Comments;
                examine.PassedAdminName = currentUser.UserName;
                examine.PassedTime = DateTime.Now.ToCstTime();
                examine.IsPassed = false;
                examine = await _examineRepository.UpdateAsync(examine);
                //修改以其为前置审核的审核状态
                if (await _examineRepository.GetAll().AnyAsync(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id))
                {
                    var temp = _examineRepository.GetAll().Where(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id);
                    foreach (var item in temp)
                    {
                        item.IsPassed = false;
                        item.Comments = "其前置审核被驳回";
                        item.PassedTime = DateTime.Now.ToCstTime();
                        item.PassedAdminName = currentUser.UserName;
                        await _examineRepository.UpdateAsync(item);
                    }
                }
            }

            //给用户发送通知
            if (examine.IsPassed == false)
            {
                await _messageRepository.InsertAsync(new Message
                {
                    Title = (examine.IsPassed ?? false) ? "编辑通过提醒" : "编辑驳回提醒",
                    PostTime = DateTime.Now.ToCstTime(),
                    Image = "default/logo.png",
                    Rank = "系统",
                    Text = "你的『" + examine.Operation.GetDisplayName() + "』操作被『" + currentUser.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                    Link = "home/examined/" + examine.Id,
                    LinkTitle = "第" + examine.Id + "条审核记录",
                    Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                    ApplicationUserId = examine.ApplicationUser.Id
                });

            }

            return new Result { Successful = true };
        }

        /// <summary>
        /// 获取编辑记录概览
        /// </summary>
        /// <param name="id">要对比的编辑记录</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ExaminesOverviewViewModel>> GetExaminesOverview(long id)
        {
            var examine = await _examineRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && s.IsPassed == true);
            if (examine == null)
            {
                return NotFound("无法找到该审核");
            }

            var model = new ExaminesOverviewViewModel();
            var examines = new List<Examine>();
            if (examine.EntryId != null)
            {
                var entry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                if (entry == null)
                {
                    return NotFound("无法找到审核记录对应的词条");
                }

                model.ObjectId = entry.Id;
                model.ObjectName = entry.DisplayName;
                model.ObjectBriefIntroduction = entry.BriefIntroduction;
                model.Image = (entry.Type == EntryType.Game || entry.Type == EntryType.ProductionGroup) ? _appHelper.GetImagePath(entry.MainPicture, "app.png") : _appHelper.GetImagePath(entry.Thumbnail, "user.png");
                model.IsThumbnail = (entry.Type == EntryType.Game || entry.Type == EntryType.ProductionGroup) ? false : true;
                model.Type = ExaminedNormalListModelType.Entry;

                examines = await _examineRepository.GetAll().AsNoTracking()
                   .Include(s => s.ApplicationUser)
                   .Include(s => s.Entry)
                   .Where(s => s.EntryId == entry.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
            }
            else if (examine.ArticleId != null)
            {
                var article = await _articleRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                if (article == null)
                {
                    return NotFound("无法找到审核记录对应的文章");
                }

                model.ObjectId = article.Id;
                model.ObjectName = article.DisplayName;
                model.ObjectBriefIntroduction = article.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(article.MainPicture, "app.png");
                model.IsThumbnail = false;
                model.Type = ExaminedNormalListModelType.Article;

                examines = await _examineRepository.GetAll().AsNoTracking()
                    .Include(s => s.ApplicationUser)
                    .Include(s => s.Article)
                    .Where(s => s.ArticleId == article.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
            }
            else if (examine.PeripheryId != null)
            {

                var periphery = await _peripheryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
                if (periphery == null)
                {
                    return NotFound("无法找到审核记录对应的周边");
                }

                model.ObjectId = periphery.Id;
                model.ObjectName = periphery.DisplayName;
                model.ObjectBriefIntroduction = periphery.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(periphery.MainPicture, "app.png");
                model.IsThumbnail = false;
                model.Type = ExaminedNormalListModelType.Periphery;

                examines = await _examineRepository.GetAll().AsNoTracking()
                   .Include(s => s.ApplicationUser)
                   .Include(s => s.Periphery)
                   .Where(s => s.PeripheryId == periphery.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
            }
            else if (examine.TagId != null)
            {

                var tag = await _tagRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == examine.TagId);
                if (tag == null)
                {
                    return NotFound("无法找到审核记录对应的标签");
                }

                model.ObjectId = tag.Id;
                model.ObjectName = tag.Name;
                model.ObjectBriefIntroduction = tag.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(tag.MainPicture, "app.png");
                model.IsThumbnail = false;
                model.Type = ExaminedNormalListModelType.Tag;

                examines = await _examineRepository.GetAll().AsNoTracking()
                    .Include(s => s.ApplicationUser)
                    .Include(s => s.Tag)
                    .Where(s => s.TagId == tag.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();
            }
            else
            {
                return BadRequest("无效的类型");
            }



            var examinedView = new ExamineViewModel();

            foreach (var item in examines)
            {
                if (await _examineService.GetExamineView(examinedView, item))
                {
                    model.Examines.Add(new EditRecordAloneViewModel
                    {
                        ApplyTime = item.ApplyTime,
                        Comments = item.Comments,
                        EditOverview = examinedView.EditOverview,
                        Id = item.Id,
                        Note = item.Note,
                        Operation = item.Operation,
                        PassedAdminName = item.PassedAdminName,
                        PassedTime = item.PassedTime.Value,
                        UserId = item.ApplicationUserId,
                        IsSelected = item.Id == id,
                        UserName = item.ApplicationUser.UserName
                    });
                }

            }

            return model;
        }

        /// <summary>
        /// 编辑用户监视词条列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<Result>> EditUserMonitors(EditUserMonitorsModel model)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return NotFound();
            }

            if (model.InMonitor)
            {
                //查找已监视的项目
                var existIds = await _userMonitorsRepository.GetAll().Where(s => s.EntryId != null && model.Ids.Contains(s.EntryId.Value)).Select(s => s.Id).ToListAsync();

                foreach (var item in model.Ids.Where(s => existIds.Contains(s) == false))
                {
                    await _userMonitorsRepository.InsertAsync(new UserMonitor
                    {
                        ApplicationUserId = user.Id,
                        CreateTime = DateTime.Now.ToCstTime(),
                        EntryId = item
                    });
                }
            }
            else
            {
                await _userMonitorsRepository.DeleteRangeAsync(s => s.ApplicationUserId == user.Id && (s.EntryId == null || model.Ids.Contains(s.EntryId.Value)));
            }

            return new Result { Successful = true };
           
        }

        /// <summary>
        /// 编辑用户审阅编辑记录列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<Result>> EditUserReviewEditRecordState(EditUserReviewEditRecordStateModel model)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return NotFound();
            }

            //查找已监视的项目
            var existIds = await _userReviewEditRecordRepository.GetAll().AsNoTracking()
                .Include(s=>s.Examine)
                .Where(s => s.ApplicationUserId == user.Id && s.ExamineId != null && model.ExamineIds.Contains(s.ExamineId.Value))
                .Select(s => s.ExamineId)
                .ToListAsync();

            //添加新项目 试着用全部添加的方式
            //foreach (var item in model.ExamineIds.Where(s => existIds.Contains(s) == false))
            //{
            //    await _userReviewEditRecordRepository.InsertAsync(new UserReviewEditRecord
            //    {
            //        ApplicationUserId = user.Id,
            //        ReviewedTime = DateTime.Now.ToCstTime(),
            //        ExamineId = item,
            //        State = model.State,
            //    });
            //}

            //修改旧项目
            _ = await _userReviewEditRecordRepository.GetRangeUpdateTable()
                .Where(s => s.ApplicationUserId == user.Id && s.ExamineId != null && model.ExamineIds.Contains(s.ExamineId.Value))
                .Set(s => s.State, b => model.State)
                .ExecuteAsync();


            return new Result { Successful = true };

        }

        /// <summary>
        /// 获取用户未读的监视词条编辑记录
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<UserContentCenterViewModel>> GetUserContentCenterView()
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserContentCenterViewModel();

            //获取用户编辑信息
            model.UserEditInfor = await _userService.GetUserEditInforBindModel(user);

            //获取未读的审核记录
            var unreadExamineIds =await _userReviewEditRecordRepository.GetAll().AsNoTracking().Include(s=>s.Examine).Where(s => s.ApplicationUserId == user.Id && s.State == EditRecordReviewState.UnRead && s.ExamineId != null).Select(s=>s.ExamineId).ToListAsync();

            var unreadExamines = _examineRepository.GetAll().AsNoTracking().Where(s => unreadExamineIds.Contains(s.Id));

            model.UnReviewExamines = await _examineService.GetExaminesToNormalListAsync(unreadExamines, true);

            //获取未读词条
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => unreadExamines.GroupBy(s => s.EntryId.Value).Select(s => s.First().EntryId.Value).Contains(s.Id))
                .ToListAsync();

            foreach(var item in entries)
            {
                model.UnReviewEntries.Add(_appHelper.GetEntryInforTipViewModel(item));
            }

            //获取待审核记录
            var allExamines = _examineRepository.GetAll().Include(s => s.ApplicationUser).AsNoTracking();
            //若不是管理员 则检查认证词条
            if (await _userManager.IsInRoleAsync(user, "Admin") == false)
            {
                var userCertification = await _userCertificationRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.EntryId != null);
                if (userCertification == null)
                {
                    allExamines = allExamines.Where(s => false);
                }
                else
                {
                    allExamines = allExamines.Where(s => s.EntryId == userCertification.EntryId);
                }
            }
           
           
                model.PendingExamines = await _examineService.GetExaminesToNormalListAsync(allExamines.Where(s => s.IsPassed == null),true);
            


            return model;
        }

        /// <summary>
        /// 获取用户所有监视列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListUserMonitorAloneModel>>> GetUserMonitorListAsync(UserMonitorsPagesInfor input)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return NotFound();
            }

            var dtos = await _editRecordService.GetPaginatedResult(input.Options, input.SearchModel, user.Id);

            return dtos;
        }

        /// <summary>
        /// 获取所有审阅列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListUserReviewEditRecordAloneModel>>> GetUserReviewEditRecordListAsync(UserReviewEditRecordsPagesInfor input)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return NotFound();
            }

            var dtos = await _editRecordService.GetPaginatedResult(input.Options, input.SearchModel, user.Id);

            return dtos;
        }

        /// <summary>
        /// 获取用户有权限查看的所有审核记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListExamineAloneModel>>> GetExamineListAsync(ExaminesPagesInfor input)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var dtos = await _examineService.GetPaginatedResult(input.Options, input.SearchModel, user);

            return dtos;
        }


    }
}
