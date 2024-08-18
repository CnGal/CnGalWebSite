using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.Comments.Dtos;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Lotteries;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.ExamineModel.Comments;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.News;
using CnGalWebSite.Helper.ViewModel.Comments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/comments/[action]")]
    public class CommentsAPIController : ControllerBase
    {

        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly ICommentService _commentService;
        private readonly IExamineService _examineService;
        private readonly IAppHelper _appHelper;
        private readonly IEditRecordService _editRecordService;
        private readonly IQueryService _queryService;
        private readonly ILotteryService _lotteryService;

        public CommentsAPIController(IRepository<ApplicationUser, string> userRepository, ICommentService commentService, IRepository<Video, long> videoRepository, IQueryService queryService,
        IRepository<Comment, long> commentRepository, IRepository<Periphery, long> peripheryRepository, IRepository<Lottery, long> lotteryRepository, IEditRecordService editRecordService, ILotteryService lotteryService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Vote, long> voteRepository, IExamineService examineService, IRepository<Examine, long> examineRepository,
        IRepository<Entry, int> entryRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _commentRepository = commentRepository;
            _commentService = commentService;

            _userRepository = userRepository;
            _peripheryRepository = peripheryRepository;
            _voteRepository = voteRepository;
            _lotteryRepository = lotteryRepository;
            _examineService = examineService;
            _examineRepository = examineRepository;
            _editRecordService = editRecordService;
            _videoRepository = videoRepository;
            _queryService = queryService;
            _lotteryService = lotteryService;
        }

        [AllowAnonymous]
        [HttpGet("{type}/{id}")]
        public async Task<ActionResult<CommentCacheModel>> GetCommentsAsync(int type, string id)
        {
            var commentType = (CommentType)type;


            //如果是用户或文章的评论列表 需要识别 作者和本人 还需要判断评论区是否被关闭
            var ascriptionUserId = "";
            var rankName = "";
            if (commentType == CommentType.CommentArticle)
            {
                var article = await _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).FirstOrDefaultAsync(s => s.Id == long.Parse(id));
                if (article != null)
                {
                    ascriptionUserId = article.CreateUser.Id;
                    rankName = "作者";
                    //判断是否被关闭
                    if (article.CanComment == false)
                    {
                        return NotFound("评论区被关闭");
                    }
                }
            }
            else if (commentType == CommentType.CommentUser)
            {
                var userSpace = await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
                if (userSpace != null)
                {
                    ascriptionUserId = id;
                    rankName = "本人";

                    //判断是否被关闭
                    if (userSpace.CanComment == false)
                    {
                        return NotFound("评论区被关闭");
                    }
                }
            }
            else if (commentType == CommentType.CommentEntries)
            {
                var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == int.Parse(id));
                if (entry != null)
                {
                    //判断是否被关闭
                    if (entry.CanComment == false)
                    {
                        return NotFound("评论区被关闭");
                    }
                }
            }
            else if (commentType == CommentType.CommentPeriphery)
            {
                var periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == int.Parse(id));
                if (periphery != null)
                {
                    //判断是否被关闭
                    if (periphery.CanComment == false)
                    {
                        return NotFound("评论区被关闭");
                    }
                }
            }
            else if (commentType == CommentType.CommentVote)
            {
                var vote = await _voteRepository.FirstOrDefaultAsync(s => s.Id == int.Parse(id));
                if (vote != null)
                {
                    //判断是否被关闭
                    if (vote.CanComment == false)
                    {
                        return NotFound("评论区被关闭");
                    }
                }
            }
            else if (commentType == CommentType.CommentLottery)
            {
                var lottery = await _lotteryRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(id));
                if (lottery != null)
                {
                    //判断是否被关闭
                    if (lottery.CanComment == false)
                    {
                        return NotFound("评论区被关闭");
                    }
                }
            }
            else if (commentType == CommentType.CommentVideo)
            {
                var video = await _videoRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(id));
                if (video != null)
                {
                    //判断是否被关闭
                    if (video.CanComment == false)
                    {
                        return NotFound("评论区被关闭");
                    }
                }
            }

            //获取审核预览
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            var examinesComments = new List<Comment>();
            if (user != null)
            {
                //获取审核记录
                var examines = await _examineRepository.GetAllListAsync(s => s.CommentId != null && s.ApplicationUserId == user.Id && (s.Operation == Operation.PubulishComment) && s.IsPassed == null);

                foreach (var item in examines)
                {
                    var temp = new Comment
                    {
                        ApplicationUser = user,
                        InverseParentCodeNavigation = new List<Comment>(),
                        Id = item.CommentId.Value
                    };
                    examinesComments.Add(temp);
                    await _commentService.UpdateCommentDataAsync(temp, item);
                }
            }

            var dtos = await _commentService.GetComments(commentType, id, rankName, ascriptionUserId, examinesComments);
            return new CommentCacheModel
            {
                Items = dtos
            };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> PublishCommentAsync(PublishCommentModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            Article article = null;
            Entry entry = null;
            Periphery periphery = null;
            Vote vote = null;
            Video video = null;
            Lottery lottery = null;
            UserSpaceCommentManager userSpace = null;
            Comment replyComment = null;
            ApplicationUser userTemp = null;
            long tempId = 0;
            if (model.Type != CommentType.CommentUser)
            {
                tempId = long.Parse(model.ObjectId);
            }
            //判断当前是否能够编辑
            switch (model.Type)
            {
                case CommentType.CommentArticle:
                    article = await _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).FirstOrDefaultAsync(s => s.Id == tempId);
                    if (article == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该文章，Id" + model.ObjectId };
                    }
                    if (article.CanComment == false && article.CreateUserId != user.Id)
                    {
                        return new Result { Successful = false, Error = "该文章作者不允许评论" };
                    }
                    break;
                case CommentType.CommentEntries:
                    entry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(model.ObjectId));
                    if (entry == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该词条，Id" + model.ObjectId };
                    }
                    if (entry.CanComment == false)
                    {
                        return new Result { Successful = false, Error = "该词条不允许评论" };
                    }
                    break;
                case CommentType.CommentPeriphery:
                    periphery = await _peripheryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(model.ObjectId));
                    if (periphery == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该周边，Id" + model.ObjectId };
                    }
                    if (periphery.CanComment == false)
                    {
                        return new Result { Successful = false, Error = "该周边不允许评论" };
                    }
                    break;
                case CommentType.CommentVote:
                    vote = await _voteRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(model.ObjectId));
                    if (vote == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该投票，Id" + model.ObjectId };
                    }
                    if (vote.CanComment == false)
                    {
                        return new Result { Successful = false, Error = "该投票不允许评论" };
                    }
                    break;
                case CommentType.CommentLottery:
                    lottery = await _lotteryRepository.GetAll().Include(s => s.Users).AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(model.ObjectId));
                    if (lottery == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该抽奖，Id" + model.ObjectId };
                    }
                    if (lottery.CanComment == false)
                    {
                        return new Result { Successful = false, Error = "该抽奖不允许评论" };
                    }
                    break;
                case CommentType.CommentVideo:
                    video = await _videoRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == long.Parse(model.ObjectId));
                    if (video == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该视频，Id" + model.ObjectId };
                    }
                    if (video.CanComment == false)
                    {
                        return new Result { Successful = false, Error = "该视频不允许评论" };
                    }
                    break;
                case CommentType.CommentUser:
                    userTemp = await _userRepository.GetAll().Include(s => s.UserSpaceCommentManager).FirstOrDefaultAsync(s => s.Id == model.ObjectId);
                    if (userTemp == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该用户，Id" + model.ObjectId };
                    }
                    if (userTemp.CanComment == false && userTemp.Id != user.Id)
                    {
                        return new Result { Successful = false, Error = "该用户不允许留言" };
                    }
                    if (userTemp.UserSpaceCommentManager == null)
                    {
                        userTemp.UserSpaceCommentManager = new UserSpaceCommentManager();
                        userTemp = await _userRepository.UpdateAsync(userTemp);
                    }
                    userSpace = userTemp.UserSpaceCommentManager;
                    _userRepository.Clear();
                    break;
                case CommentType.ReplyComment:
                    replyComment = await _commentRepository.GetAll().AsNoTracking().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == tempId);
                    if (replyComment == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该评论，Id" + model.ObjectId };
                    }
                    break;
                default:
                    return NotFound();
            }

            //先放入数据库 为了获得Id
            var comment = await _commentRepository.InsertAsync(new Comment
            {
                CommentTime = DateTime.Now.ToCstTime(),
                ApplicationUserId = user.Id,
                LotteryId = lottery?.Id,
                Type = model.Type,
            });
            var commentText = new CommentText
            {
                Text = model.Text,
                CommentTime = DateTime.Now.ToCstTime(),
                Type = model.Type,
                PubulicUserId = user.Id,
                ObjectId = model.ObjectId
            };

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(comment, user, commentText, Operation.PubulishComment, "");
            //判断是否需要参与抽奖
            if (lottery != null && lottery.ConditionType == LotteryConditionType.CommentLottery)
            {
                try
                {
                    await _lotteryService.AddUserToLottery(lottery, user, HttpContext, model.Identification);
                }
                catch
                { }
            }

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditArticleCanCommentAsync(EditArticleCanCommentModel model)
        {
            await _articleRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.CanComment, b => model.CanComment));
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditPeripheryCanCommentAsync(EditPeripheryCanCommentModel model)
        {
            await _peripheryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.CanComment, b => model.CanComment));
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditVoteCanCommentAsync(EditVoteCanCommentModel model)
        {
            await _voteRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.CanComment, b => model.CanComment));
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditLotteryCanCommentAsync(EditLotteryCanCommentModel model)
        {
            await _lotteryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.CanComment, b => model.CanComment));
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditVideoCanCommentAsync(EditLotteryCanCommentModel model)
        {
            await _videoRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.CanComment, b => model.CanComment));
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditEntryCanCommentAsync(EditEntryCanCommentModel model)
        {
            await _entryRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.CanComment, b => model.CanComment));

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditSpaceCanCommentAsync(EditSpaceCanComment model)
        {
            await _userRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.CanComment, b => model.CanComment));


            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenCommentAsync(HiddenCommentModel model)
        {

            await _commentRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.IsHidden, b => model.IsHidden));

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserHiddenCommentAsync(HiddenCommentModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            foreach (var item in model.Ids)
            {
                if (await _commentService.IsUserHavePermissionForCommmentAsync(item, user))
                {
                    await _commentRepository.GetAll().Where(s => s.Id == item).ExecuteUpdateAsync(s => s.SetProperty(s => s.IsHidden, b => true));
                }
                else
                {
                    return new Result { Successful = false, Error = "该评论不存在，或你没有权限删除该评论" };
                }
            }
            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<CommentOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Comment, long>(_commentRepository.GetAll().AsSingleQuery().Where(s => string.IsNullOrWhiteSpace(s.Text) == false)
                .Include(s => s.ApplicationUser)
                .Include(s => s.Entry)
                .Include(s => s.Article)
                .Include(s => s.Periphery)
                .Include(s => s.UserSpaceCommentManager).ThenInclude(s => s.ApplicationUser)
                .Include(s => s.Vote)
                .Include(s => s.Lottery)
                .Include(s => s.ParentCodeNavigation), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || ((s.Text.Contains(model.SearchText))));

            return new QueryResultModel<CommentOverviewModel>
            {
                Items = await items.Select(s => new CommentOverviewModel
                {
                    Id = s.Id,
                    Type = s.Type,
                    CommentTime = s.CommentTime,
                    Text = s.Text,
                    UserId = s.ApplicationUser.Id,
                    UserName = s.ApplicationUser.UserName,
                    IsHidden = s.IsHidden,
                    Priority = s.Priority,
                    ObjectId = s.Entry != null ? s.Entry.Id.ToString() : s.Article != null ? s.Article.Id.ToString() : s.Periphery != null ? s.Periphery.Id.ToString() : s.Video != null ? s.Video.Id.ToString() : s.UserSpaceCommentManager != null ? s.UserSpaceCommentManager.ApplicationUser.Id : s.Vote != null ? s.Vote.Id.ToString() : s.Lottery != null ? s.Lottery.Id.ToString() : null,
                    ObjectName = s.Entry != null ? s.Entry.Name : s.Article != null ? s.Article.Name : s.Periphery != null ? s.Periphery.Name : s.Video != null ? s.Video.Name : s.UserSpaceCommentManager != null ? s.UserSpaceCommentManager.ApplicationUser.UserName : s.Vote != null ? s.Vote.Name : s.Lottery != null ? s.Lottery.Name : s.ParentCodeNavigation != null ? "回复：Id" + s.ParentCodeNavigation.Id : null
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditCommentPriorityAsync(EditCommentPriorityViewModel model)
        {
            await _commentRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Successful = true };
        }

    }
}
