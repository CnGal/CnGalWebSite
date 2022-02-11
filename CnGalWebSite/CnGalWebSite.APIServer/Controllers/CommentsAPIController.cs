using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.Comments.Dtos;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.DataModel.ViewModel.Entries;
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
    [Route("api/comments/[action]")]
    public class CommentsAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly ICommentService _commentService;
        private readonly IAppHelper _appHelper;

        public CommentsAPIController(UserManager<ApplicationUser> userManager, IRepository<ApplicationUser, string> userRepository, ICommentService commentService,
            IRepository<Comment, long> commentRepository, IRepository<Periphery, long> peripheryRepository, IRepository<Lottery, long> lotteryRepository,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Vote, long> voteRepository,
        IRepository<Entry, int> entryRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _commentRepository = commentRepository;
            _commentService = commentService;
            _userManager = userManager;
            _userRepository = userRepository;
            _peripheryRepository = peripheryRepository;
            _voteRepository = voteRepository;
            _lotteryRepository = lotteryRepository;
        }

        [AllowAnonymous]
        [HttpGet("{type}/{id}/{currentPage}/{MaxResultCount}")]
        public async Task<ActionResult<PagedResultDto<CommentViewModel>>> GetCommentsAsync(int type, string id, int currentPage, int MaxResultCount)
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
                        return new PagedResultDto<CommentViewModel> { Data = new List<CommentViewModel>() };
                    }
                }
            }
            else if (commentType == CommentType.CommentUser)
            {
                var userSpace = await _userManager.FindByIdAsync(id);
                if (userSpace != null)
                {
                    ascriptionUserId = id;
                    rankName = "本人";

                    //判断是否被关闭
                    if (userSpace.CanComment == false)
                    {
                        return new PagedResultDto<CommentViewModel> { Data = new List<CommentViewModel>() };
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
                        return new PagedResultDto<CommentViewModel> { Data = new List<CommentViewModel>() };
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
                        return new PagedResultDto<CommentViewModel> { Data = new List<CommentViewModel>() };
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
                        return new PagedResultDto<CommentViewModel> { Data = new List<CommentViewModel>() };
                    }
                }
            }
            else if (commentType == CommentType.CommentLottery)
            {
                var lottery = await _lotteryRepository.FirstOrDefaultAsync(s => s.Id == int.Parse(id));
                if (lottery != null)
                {
                    //判断是否被关闭
                    if (lottery.CanComment == false)
                    {
                        return new PagedResultDto<CommentViewModel> { Data = new List<CommentViewModel>() };
                    }
                }
            }

            var input = new GetCommentInput
            {
                CurrentPage = currentPage,
                MaxResultCount = MaxResultCount,
                ScreeningConditions = "全部"
            };
            var dtos = await _commentService.GetPaginatedResult(input, commentType, id, rankName, ascriptionUserId);
            return dtos;
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
                    article = await _articleRepository.GetAll().Include(s => s.CreateUser).FirstOrDefaultAsync(s => s.Id == tempId);
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
                    entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(model.ObjectId));
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
                    periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(model.ObjectId));
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
                    vote = await _voteRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(model.ObjectId));
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
                    lottery = await _lotteryRepository.FirstOrDefaultAsync(s => s.Id == long.Parse(model.ObjectId));
                    if (lottery == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该抽奖，Id" + model.ObjectId };
                    }
                    if (lottery.CanComment == false)
                    {
                        return new Result { Successful = false, Error = "该抽奖不允许评论" };
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
                    break;
                case CommentType.ReplyComment:
                    replyComment = await _commentRepository.GetAll().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == tempId);
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
                CommentTime = DateTime.Now.ToCstTime()
            });
            var commentText = new CommentText
            {
                Text = model.Text,
                CommentTime = DateTime.Now.ToCstTime(),
                Type = model.Type,
                PubulicUserId = user.Id,
                ObjectId = model.ObjectId
            };
            //序列化JSON
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, commentText);
                resulte = text.ToString();
            }

            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Admin") == true)
            {
                await _appHelper.ExaminePublishCommentTextAsync(comment, commentText);
                await _appHelper.UniversalCommentExaminedAsync(comment, user, true, resulte, Operation.PubulishComment, "");
                await _appHelper.AddUserContributionValueAsync(user.Id, comment.Id, Operation.PubulishComment);

            }
            else
            {
                await _appHelper.UniversalCommentExaminedAsync(comment, user, false, resulte, Operation.PubulishComment, "");
            }

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditArticleCanCommentAsync(EditArticleCanCommentModel model)
        {
            await _articleRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.CanComment, b => model.CanComment).ExecuteAsync();
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditPeripheryCanCommentAsync(EditPeripheryCanCommentModel model)
        {
            await _peripheryRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.CanComment, b => model.CanComment).ExecuteAsync();
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditVoteCanCommentAsync(EditVoteCanCommentModel model)
        {
            await _voteRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.CanComment, b => model.CanComment).ExecuteAsync();
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditEntryCanCommentAsync(EditEntryCanCommentModel model)
        {
            await _entryRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.CanComment, b => model.CanComment).ExecuteAsync();

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditSpaceCanCommentAsync(EditSpaceCanComment model)
        {
            await _userRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.CanComment, b => model.CanComment).ExecuteAsync();


            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenCommentAsync(HiddenCommentModel model)
        {

            await _commentRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> DeleteCommentAsync(DeleteCommentModel model)
        {
            foreach (var item in model.Ids)
            {
                await _appHelper.DeleteComment(item);
            }

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserDeleteCommentAsync(DeleteCommentModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            foreach (var item in model.Ids)
            {
                if (await _appHelper.IsUserHavePermissionForCommmentAsync(item, user))
                {
                    //删除
                    await _appHelper.DeleteComment(item);
                }
                else
                {
                    return new Result { Successful = false, Error = "该评论不存在，或你没有权限删除该评论" };
                }
            }
            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserHiddenCommentAsync(HiddenCommentModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            foreach (var item in model.Ids)
            {
                if (await _appHelper.IsUserHavePermissionForCommmentAsync(item, user))
                {
                    await _commentRepository.GetRangeUpdateTable().Where(s => s.Id == item).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();
                }
                else
                {
                    return new Result { Successful = false, Error = "该评论不存在，或你没有权限删除该评论" };
                }
            }
            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListCommentAloneModel>>> GetCommentListNormalAsync(CommentsPagesInfor input)
        {
            var dtos = await _commentService.GetPaginatedResult(input.Options, input.SearchModel, input.Type, input.ObjectId);

            return dtos;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditCommentPriorityAsync(EditCommentPriorityViewModel model)
        {
            await _commentRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.Priority, b => b.Priority + model.PlusPriority).ExecuteAsync();

            return new Result { Successful = true };
        }

    }
}
