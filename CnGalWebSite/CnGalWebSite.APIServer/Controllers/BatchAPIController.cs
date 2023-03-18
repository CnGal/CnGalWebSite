using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ImportModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Batch;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/batch/[action]")]
    public class BatchAPIController : ControllerBase
    {
        
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<FriendLink, int> _friendLinkRepository;
        private readonly IRepository<Carousel, int> _carouselRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<HistoryUser, int> _historyUserRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;
        private readonly IUserService _userService;


        public BatchAPIController(IRepository<HistoryUser, int> historyUserRepository, IExamineService examineService, IRepository<Periphery, long> peripheryRepository, IRepository<ApplicationUser, string> userRepository,
        IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor, IUserService userService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Video, long> videoRepository)
        {
            
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _friendLinkRepository = friendLinkRepository;
            _carouselRepository = carouselRepositor;
            _historyUserRepository = historyUserRepository;
            _examineService = examineService;
            _peripheryRepository = peripheryRepository;
            _userService = userService;
            _videoRepository = videoRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UpdateEntryDataAsync(ListEntryAloneModel model)
        {
            //查找词条
            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (entry == null)
            {
                return new Result { Successful = false, Error = $"未找到Id：{model.Id}的词条" };
            }
            //修改数据
            entry.Name = model.Name;
            entry.Type = (EntryType)model.Type;
            entry.DisplayName = model.DisplayName;
            entry.BriefIntroduction = model.BriefIntroduction;
            entry.Priority = model.Priority;
            entry.IsHidden = model.IsHidden;
            entry.CanComment = model.CanComment;
            //保存
            await _entryRepository.UpdateAsync(entry);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UpdateArticleDataAsync(ListArticleAloneModel model)
        {
            //查找词条
            var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (article == null)
            {
                return new Result { Successful = false, Error = $"未找到Id：{model.Id}的文章" };
            }
            //修改数据
            article.Name = model.Name;
            article.DisplayName = model.DisplayName;
            article.Type = (ArticleType)model.Type;
            article.CreateTime = model.CreateTime;
            article.LastEditTime = model.LastEditTime;
            article.ReaderCount = model.ReaderCount;
            article.OriginalAuthor = model.OriginalAuthor;
            article.OriginalLink = model.OriginalLink;
            article.Priority = model.Priority;
            article.IsHidden = model.IsHidden;
            article.CanComment = model.CanComment;

            //保存
            await _articleRepository.UpdateAsync(article);
            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UpdateAuthorAsync(UpdateAuthorModel model)
        {
            var user = await _userRepository.FirstOrDefaultAsync(s => s.Id == model.UserId);
            if (user == null)
            {
                return new Result { Successful = false, Error = "用户『Id：" + model.UserId + "』不存在" };
            }

            foreach (var item in model.ArticleIds)
            {
                var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == item);
                if (article == null)
                {
                    return new Result { Successful = false, Error = "文章『Id：" + item + "』不存在" };
                }
                //修改文章作者
                article.CreateUserId = user.Id;
                await _articleRepository.UpdateAsync(article);
            }

            foreach (var item in model.VideoIds)
            {
                var video = await _videoRepository.FirstOrDefaultAsync(s => s.Id == item);
                if (video == null)
                {
                    return new Result { Successful = false, Error = "视频『Id：" + item + "』不存在" };
                }
                //修改文章作者
                video.CreateUserId = user.Id;
                await _videoRepository.UpdateAsync(video);
            }



            //更新用户积分
            await _userService.UpdateUserIntegral(user);
            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> ImportEntryDataAsync(Entry model)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //不进行完整数据检查 管理员负责确保数据无误 不建议批量导入数据
            if (model.Id != 0)
            {
                return new Result { Successful = false, Error = "导入数据时，不支持指定Id" };
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new Result { Successful = false, Error = "名称不能为空" };
            }
            //不能重名
            if (await _entryRepository.GetAll().AnyAsync(s => s.Name == model.Name))
            {
                return new Result { Successful = false, Error = "词条名称『" + model.Name + "』重复，请尝试使用显示名称" };
            }
            //导入数据
            try
            {
                await _examineService.AddNewEntryExaminesAsync(model, user, "批量导入历史数据");
            }
            catch (Exception exc)
            {
                return new Result { Successful = false, Error = "为词条创建审核记录时发生异常『" + exc.Message + "』" };
            }

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> ImportPeripheryDataAsync(Periphery model)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //不进行完整数据检查 管理员负责确保数据无误 不建议批量导入数据
            if (model.Id != 0)
            {
                return new Result { Successful = false, Error = "导入数据时，不支持指定Id" };
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new Result { Successful = false, Error = "名称不能为空" };
            }
            //不能重名
            if (await _peripheryRepository.GetAll().AnyAsync(s => s.Name == model.Name))
            {
                return new Result { Successful = false, Error = "周边名称『" + model.Name + "』重复，请尝试使用显示名称" };
            }
            //导入数据
            try
            {
                await _examineService.AddNewPeripheryExaminesAsync(model, user, "批量导入历史数据");
            }
            catch (Exception exc)
            {
                return new Result { Successful = false, Error = "为周边创建审核记录时发生异常『" + exc.Message + "』" };
            }

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> ImportArticleDataAsync(Article model)
        {
            //不进行完整数据检查 管理员负责确保数据无误 不建议批量导入数据
            if (model.Id != 0)
            {
                return new Result { Successful = false, Error = "导入数据时，不支持指定Id" };
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new Result { Successful = false, Error = "名称不能为空" };
            }
            //不能重名
            if (await _articleRepository.CountAsync(s => s.Name == model.Name) > 0)
            {
                return new Result { Successful = false, Error = "文章名称名称『" + model.Name + "』重复，请尝试使用显示名称" };
            }
            //关联创建者
            //检查关联用户是否存在
            ApplicationUser user = null;
            if (string.IsNullOrWhiteSpace(model.CreateUserId) == false)
            {
                //判断是否为历史用户Id
                if (model.CreateUserId.Length < 6)
                {
                    var historyUser = await _historyUserRepository.FirstOrDefaultAsync(s => s.UserIdentity == model.CreateUserId);
                    if (historyUser != null)
                    {
                        //一定会用用户名进行关联
                        user = await _userRepository.FirstOrDefaultAsync(s => s.UserName == historyUser.UserName);
                    }
                }
                else
                {
                    user = await _userRepository.FirstOrDefaultAsync(s => s.Id == model.CreateUserId);
                }

            }
            //不存在则使用导入者Id
            if (user == null)
            {
                user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            }
            model.CreateUser = user;
            model.CreateUserId = user.Id;
            //判断是否添加转载信息
            if (user.UserName == model.OriginalAuthor)
            {
                model.OriginalAuthor = "";
                model.OriginalLink = "";
            }
            //导入数据
            try
            {
                await _examineService.AddNewArticleExaminesAsync(model, user, "批量导入历史数据");
                //更新用户积分
                await _userService.UpdateUserIntegral(user);
            }
            catch (Exception exc)
            {
                return new Result { Successful = false, Error = "为文章创建审核记录时发生异常『" + exc.Message + "』" };
            }

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> ImportFriendLinkDataAsync(FriendLink model)
        {
            //不进行完整数据检查 管理员负责确保数据无误 不建议批量导入数据

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new Result { Successful = false, Error = "名称不能为空" };
            }
            if (string.IsNullOrWhiteSpace(model.Link))
            {
                return new Result { Successful = false, Error = "链接不能为空" };
            }

            //导入数据
            await _friendLinkRepository.InsertAsync(model);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> ImportCarouselDataAsync(Carousel model)
        {
            //不进行完整数据检查 管理员负责确保数据无误 不建议批量导入数据

            if (string.IsNullOrWhiteSpace(model.Image))
            {
                return new Result { Successful = false, Error = "图片不能为空" };
            }
            if (string.IsNullOrWhiteSpace(model.Link))
            {
                return new Result { Successful = false, Error = "链接不能为空" };
            }

            //导入数据
            await _carouselRepository.InsertAsync(model);

            return new Result { Successful = true };
        }
    }
}
