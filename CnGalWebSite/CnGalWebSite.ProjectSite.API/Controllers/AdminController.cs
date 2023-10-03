using CnGalWebSite.Core.Helpers;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Carousels;
using CnGalWebSite.ProjectSite.Models.ViewModels.FriendLinks;
using CnGalWebSite.ProjectSite.Models.ViewModels.Share;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IRepository<Carousel, long> _carouselRepository;
        private readonly IRepository<FriendLink, long> _friendLinkRepository;
        private readonly IQueryService _queryService;

        public AdminController(IRepository<Carousel, long> carouselRepository, IRepository<FriendLink, long> friendLinkRepository, IQueryService queryService)
        {
            _carouselRepository = carouselRepository;
            _friendLinkRepository = friendLinkRepository;
            _queryService = queryService;
        }

        /// <summary>
        /// 获取服务器动态数据概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ServerRealTimeOverviewModel>> GetServerRealTimeDataOverview()
        {
            return await Task.FromResult(SystemEnvironmentHelper.GetServerRealTimeDataOverview());
        }

        /// <summary>
        /// 获取服务器静态数据概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ServerStaticOverviewModel>> GetServerStaticDataOverview()
        {
            return await Task.FromResult(SystemEnvironmentHelper.GetServerStaticDataOverview());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<CarouselOverviewModel>> ListCarousels(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Carousel, long>(_carouselRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Note.Contains(model.SearchText)));

            return new QueryResultModel<CarouselOverviewModel>
            {
                Items = await items.Select(s => new CarouselOverviewModel
                {
                    Id = s.Id,
                    Priority = s.Priority,
                    Image = s.Image,
                    Link = s.Link,
                    Note = s.Note
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<CarouselEditModel>> EditCarouselAsync(int id)
        {
            var item = await _carouselRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null)
            {
                return NotFound("无法找到目标");
            }

            var model = new CarouselEditModel
            {
                Id = item.Id,
                Priority = item.Priority,
                Image = item.Image,
                Link = item.Link,
                Note = item.Note
            };

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditCarouselAsync(CarouselEditModel model)
        {
            Carousel item = null;
            if (model.Id == 0)
            {
                item = await _carouselRepository.InsertAsync(new Carousel
                {
                    Id = model.Id,
                    Priority = model.Priority,
                    Image = model.Image,
                    Link = model.Link,
                    Note = model.Note
                });
                model.Id = item.Id;
                _carouselRepository.Clear();
            }

            item = await _carouselRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.Id);


            if (item == null)
            {
                return new Result { Success = false, Message = "项目不存在" };
            }

            item.Id = model.Id;
            item.Priority = model.Priority;
            item.Image = model.Image;
            item.Link = model.Link;
            item.Note = model.Note;

            await _carouselRepository.UpdateAsync(item);

            return new Result { Success = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditCarouselPriorityAsync(EditPriorityModel model)
        {
            await _carouselRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Success = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<FriendLinkOverviewModel>> ListFriendLinks(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<FriendLink, int>(_friendLinkRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<FriendLinkOverviewModel>
            {
                Items = await items.Select(s => new FriendLinkOverviewModel
                {
                    Id = s.Id,
                    Priority = s.Priority,
                    Image = s.Image,
                    Link = s.Link,
                    Name = s.Name,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<FriendLinkEditModel>> EditFriendLinkAsync(int id)
        {
            var item = await _friendLinkRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null)
            {
                return NotFound("无法找到目标");
            }

            var model = new FriendLinkEditModel
            {
                Id = item.Id,
                Priority = item.Priority,
                Name = item.Name,
                Image = item.Image,
                Link = item.Link,
            };

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditFriendLinkAsync(FriendLinkEditModel model)
        {
            FriendLink item = null;
            if (model.Id == 0)
            {
                item = await _friendLinkRepository.InsertAsync(new FriendLink
                {
                    Id = model.Id,
                    Priority = model.Priority,
                    Name = model.Name,
                    Image = model.Image,
                    Link = model.Link,
                });
                model.Id = item.Id;
                _friendLinkRepository.Clear();
            }

            item = await _friendLinkRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.Id);


            if (item == null)
            {
                return new Result { Success = false, Message = "项目不存在" };
            }

            item.Id = model.Id;
            item.Priority = model.Priority;
            item.Name = model.Name;
            item.Image = model.Image;
            item.Link = model.Link;

            await _friendLinkRepository.UpdateAsync(item);

            return new Result { Success = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditFriendLinkPriorityAsync(EditPriorityModel model)
        {
            await _friendLinkRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Success = true };
        }
    }
}
