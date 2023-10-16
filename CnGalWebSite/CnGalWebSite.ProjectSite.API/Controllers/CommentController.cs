using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.API.Services.Users;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Comments;
using CnGalWebSite.ProjectSite.Models.ViewModels.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.Core.Models;
using CnGalWebSite.ProjectSite.Models.ViewModels.Share;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.ProjectSite.API.Services.Messages;
using CnGalWebSite.ProjectSite.Models.ViewModels.Messages;

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<ProjectPositionUser, long> _projectPositionUserRepository;
        private readonly IUserService _userService;
        private readonly IQueryService _queryService;
        private readonly IMessageService _messageService;

        public CommentController(IRepository<Comment, long> commentRepository, IUserService userService, IQueryService queryService, IRepository<ProjectPositionUser, long> projectPositionUserRepository,
            IMessageService messageService)
        {
            _commentRepository = commentRepository;
            _userService = userService;
            _queryService = queryService;
            _projectPositionUserRepository = projectPositionUserRepository;
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CommentViewModel>>> GetCommentsAsync(int type, long id)
        {
            var result = new List<CommentViewModel>();

            var commentType = (CommentType)type;

            //检查权限
            if (commentType == CommentType.PositionUser)
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null && !await _projectPositionUserRepository.GetAll().Include(s => s.Position).ThenInclude(s => s.Project).AnyAsync(s => s.Id == id && (s.UserId == user.Id || s.Position.Project.CreateUserId == user.Id)))
                {
                    return new List<CommentViewModel>();
                }
            }
            else
            {
                return new List<CommentViewModel>();
            }

            var comments = await _commentRepository.GetAll().AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.Type == commentType && s.ObjectId == id && s.Hide == false)
                .ToListAsync();

            foreach (var item in comments)
            {
                result.Add(await CombinationDataAsync(item));
            }

            return result;
        }

        /// <summary>
        /// 递归复制数据到视图模型
        /// </summary>
        private async Task<CommentViewModel> CombinationDataAsync(Comment comment)
        {

            var model = new CommentViewModel
            {
                CreateTime = comment.CreateTime,
                Type = comment.Type,
                Text = comment.Text,
                Id = comment.Id,
                UserInfor = _userService.GetUserInfo(comment.User)
            };

            //递归初始化子评论
            var comments = await _commentRepository.GetAll().AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.Type == CommentType.Comment && s.ObjectId == comment.Id && s.Hide == false)
                .ToListAsync();

            if (comments.Any())
            {
                foreach (var item in comments)
                {
                    model.Children.Add(await CombinationDataAsync(item));
                }
            }
            return model;

        }

        [HttpGet]
        public async Task<ActionResult<CommentEditModel>> EditAsync([FromQuery] long id)
        {
            if (id == 0)
            {
                return new CommentEditModel();
            }

            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            var item = await _commentRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == id && (s.UserId == user.Id || admin));

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new CommentEditModel
            {
                Id = id,
                ObjectId = item.ObjectId,
                Text = item.Text,
                PageId = item.PageId,
                PageType = item.PageType,
                Type = item.Type,
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditAsync(CommentEditModel model)
        {
            var vail = model.Validate();
            if (!vail.Success)
            {
                return vail;
            }
            var user = await _userService.GetCurrentUserAsync();

            Comment item = null;
            if (model.Id == 0)
            {
                item = await _commentRepository.InsertAsync(new Comment
                {
                    ObjectId = model.ObjectId,
                    Text = model.Text,
                    Type = model.Type,
                    PageId = model.PageId,
                    PageType = model.PageType,
                    UserId = user.Id,
                    CreateTime = DateTime.Now.ToCstTime(),
                });
                model.Id = item.Id;
                _commentRepository.Clear();

                //向用户发送消息
                if (model.Type == CommentType.Comment)
                {
                    var temp = await _commentRepository.FirstOrDefaultAsync(s => s.Id == model.ObjectId);
                    if (temp.UserId != user.Id)
                    {
                        await _messageService.PutMessage(new PutMessageModel
                        {
                            PageId = model.PageId,
                            PageType = model.PageType,
                            Text = $"“{user.GetName()}”回复了你：『{model.Text}』",
                            Type = MessageType.Reply,
                            UserId = temp.UserId,
                        });
                    }

                }
                else if (model.Type == CommentType.PositionUser)
                {
                    var temp = await _projectPositionUserRepository.GetAll().Include(s=>s.Position).ThenInclude(s=>s.Project).FirstOrDefaultAsync(s => s.Id == model.ObjectId);
                    if (temp.UserId != user.Id)
                    {
                        await _messageService.PutMessage(new PutMessageModel
                        {
                            PageId = model.PageId,
                            PageType = model.PageType,
                            Text = $"“{user.GetName()}”在你的应征请求下留言：『{model.Text}』",
                            Type = MessageType.Reply,
                            UserId = temp.Position.Project.CreateUserId,
                        });
                    }
                }
            }

            var admin = _userService.CheckCurrentUserRole("Admin");

            item = await _commentRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == model.Id && (s.UserId == user.Id || admin));

            if (item == null)
            {
                return new Result { Success = false, Message = "项目不存在" };
            }

            item.ObjectId = model.ObjectId;
            item.Type = model.Type;
            item.Text = model.Text;
            item.PageId = model.PageId;
            item.PageType = model.PageType;

            item.UpdateTime = DateTime.Now.ToCstTime();

            await _commentRepository.UpdateAsync(item);

            return new Result { Success = true, Message = model.Id.ToString() };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> HideAsync(HideModel model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            if (!await _commentRepository.GetAll().AnyAsync(s => s.Id == model.Id && (s.UserId == user.Id || admin)))
            {
                return new Result { Success = false, Message = "权限不足" };
            }

            await _commentRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Hide, b => model.Hide));
            return new Result { Success = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditPriorityAsync(EditPriorityModel model)
        {
            await _commentRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Success = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<CommentOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Comment, long>(_commentRepository.GetAll().AsSingleQuery().Include(s => s.User), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Text.Contains(model.SearchText) || s.User.UserName.Contains(model.SearchText)));

            return new QueryResultModel<CommentOverviewModel>
            {
                Items = await items.Select(s => new CommentOverviewModel
                {
                    Id = s.Id,
                    CreateTime = s.CreateTime,
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    Hide = s.Hide,
                    PageId = s.PageId,
                    Text = s.Text,
                    Type = s.Type,
                    PageType = s.PageType,
                    Priority = s.Priority,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
