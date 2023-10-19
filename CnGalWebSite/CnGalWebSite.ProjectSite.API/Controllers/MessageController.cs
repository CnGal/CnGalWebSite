using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.ProjectSite.API.Services.Users;
using CnGalWebSite.ProjectSite.Models.ViewModels.Share;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IUserService _userService;
        private readonly IQueryService _queryService;

        public MessageController(IRepository<Message, long> messageRepository, IUserService userService, IQueryService queryService)
        {
            _messageRepository = messageRepository;
            _userService = userService;
            _queryService = queryService;
        }

        [HttpGet]
        public async Task<MessagePageModel> Get()
        {
            var user = await _userService.GetCurrentUserAsync();

            var messages = await _messageRepository.GetAll().AsNoTracking()
                .Where(s => s.UserId == user.Id && s.Hide == false)
                .Select(s => new MessageViewModel
                {
                    Id = s.Id,
                    PageId=s.PageId,
                    PageType = s.PageType,
                    Read = s.Read,
                    Text = s.Text,
                    Type = s.Type,
                    CreateTime = s.CreateTime,
                })
                .ToListAsync();

            var model = new MessagePageModel();

            model.Read.Items = messages.Where(s => s.Read).OrderByDescending(s => s.CreateTime).ToList();
            model.UnRead.Items = messages.Where(s => !s.Read).OrderByDescending(s => s.CreateTime).ToList();

            return model;
        }

        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HideAsync(HideModel model)
        {
            await _messageRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Hide, b => model.Hide));
            return new Result { Success = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> ReadAsync(ReadMessageModel model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            await _messageRepository.GetAll().Where(s => model.Id == s.Id && (s.UserId == user.Id || admin)).ExecuteUpdateAsync(s => s.SetProperty(s => s.Read, b => model.Read));
            return new Result { Success = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<MessageOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Message, long>(_messageRepository.GetAll().AsSingleQuery().Include(s => s.User), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Text.Contains(model.SearchText) || s.User.UserName.Contains(model.SearchText)));

            return new QueryResultModel<MessageOverviewModel>
            {
                Items = await items.Select(s => new MessageOverviewModel
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
                    Read = s.Read,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpGet]
        public async Task<int> GetUnReadMessageCount()
        {
            var user = await _userService.GetCurrentUserAsync();

            return await _messageRepository.GetAll().AsNoTracking()
                .Where(s => s.UserId == user.Id && s.Hide == false && s.Read == false)
                .CountAsync();
        }

    }
}
