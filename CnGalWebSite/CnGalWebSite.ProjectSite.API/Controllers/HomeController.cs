using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Carousels;
using CnGalWebSite.ProjectSite.Models.ViewModels.FriendLinks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IRepository<Carousel, long> _carouselRepository;
        private readonly IRepository<FriendLink, long> _friendLinkRepository;

        public HomeController(IRepository<Carousel, long> carouselRepository, IRepository<FriendLink, long> friendLinkRepository)
        {
            _carouselRepository = carouselRepository;
            _friendLinkRepository = friendLinkRepository;
        }

        [HttpGet]
        public async Task<List<CarouselViewModel>> GetHomeCarouselsViewAsync()
        {
            var carouses = await _carouselRepository.GetAll().AsNoTracking().Where(s => s.Priority >= 0).OrderByDescending(s => s.Priority).ToListAsync();

            var model = new List<CarouselViewModel>();
            foreach (var item in carouses)
            {
                model.Add(new CarouselViewModel
                {
                    Image =item.Image,
                    Link = item.Link,
                    Note = item.Note,
                    Priority = item.Priority,
                    Id = item.Id,
                });
            }

            return model;
        }

        [HttpGet]
        public async Task<List<FriendLinkViewModel>> ListFriendLinks()
        {
            var model = new List<FriendLinkViewModel>();

            //获取友情置顶词条 根据优先级排序
            var entry_result4 = await _friendLinkRepository.GetAll().AsNoTracking().OrderByDescending(s => s.Priority)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (entry_result4 != null)
            {
                foreach (var item in entry_result4)
                {
                    model.Add(new FriendLinkViewModel
                    {
                        Image =item.Image,
                        Url = item.Link,
                        Name = item.Name,
                    });
                }

            }
            return model;
        }
    }
}
