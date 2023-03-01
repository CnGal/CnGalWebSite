using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Lotteries;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Videos;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;

using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Bookings;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Search;
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
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/booking/[action]")]
    public class BookingAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IRepository<Booking, long> _bookingRepository;
        private readonly IRepository<BookingUser, long> _bookingUserRepository;
        private readonly IAppHelper _appHelper;
        private readonly IEntryService _entryService;
        private readonly IArticleService _articleService;
        private readonly IVideoService _videoService;
        private readonly IExamineService _examineService;
        private readonly IPerfectionService _perfectionService;
        private readonly IEditRecordService _editRecordService;
        private readonly ILogger<BookingAPIController> _logger;
        private readonly IOperationRecordService _operationRecordService;
        private readonly ILotteryService _lotteryService;

        public BookingAPIController(UserManager<ApplicationUser> userManager, IRepository<Article, long> articleRepository, IRepository<Periphery, long> peripheryRepository, IVideoService videoService, IRepository<Lottery, long> lotteryRepository,
        IPerfectionService perfectionService, IRepository<Examine, long> examineRepository, IArticleService articleService, IEditRecordService editRecordService, ILogger<BookingAPIController> logger, IRepository<Booking, long> bookingRepository,
        IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Tag, int> tagRepository, IEntryService entryService, IExamineService examineService, IRepository<Video, long> videoRepository, IRepository<BookingUser, long> bookingUserRepository,
        IOperationRecordService operationRecordService, ILotteryService lotteryService)
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
            _articleService = articleService;
            _peripheryRepository = peripheryRepository;
            _editRecordService = editRecordService;
            _logger = logger;
            _videoService = videoService;
            _videoRepository = videoRepository;
            _lotteryRepository = lotteryRepository;
            _bookingRepository = bookingRepository;
            _bookingUserRepository = bookingUserRepository;
            _operationRecordService = operationRecordService;
            _lotteryService = lotteryService;
        }

        
        [HttpPost]
        public async Task<ActionResult<Result>> BookingGameAsync(BookingGameModel model)
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user == null)
            {
                return NotFound();
            }

            var booking = await _bookingRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.EntryId == model.GameId && s.Open);
            if(booking == null)
            {
                return new Result { Successful = false, Error = "当前预约未开启或已结束" };
            }

            var bookUser = await _bookingUserRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.BookingId == booking.Id && s.ApplicationUserId == user.Id);

            if (model.IsBooking)
            {
                if (bookUser == null)
                {
                    await _bookingUserRepository.InsertAsync(new BookingUser
                    {
                        ApplicationUserId = user.Id,
                        BookingId=booking.Id,
                        BookingTime = DateTime.Now.ToCstTime()
                    });
                }
                else
                {
                    return new Result { Successful = false, Error = "已经预约成功" };
                }
            }
            else
            {
                if (bookUser == null)
                {
                    return new Result { Successful = false, Error = "还没有预约" };
                }
                else
                {
                    await _bookingUserRepository.DeleteAsync(bookUser);
                }

            }

            //参加关联抽奖
            if (booking.LotteryId != 0 && model.IsBooking)
            {
                var lottery = await _lotteryRepository.GetAll().AsNoTracking().Include(s => s.Users).FirstOrDefaultAsync(s => s.Id == booking.LotteryId);
                try
                {
                    await _lotteryService.AddUserToLottery(lottery, user, HttpContext, model.Identification);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "BookingId:{id} User:{id} LotteryId:{id},自动参与抽奖失败", booking.Id, user.Id, lottery.Id);
                    return new Result { Successful = false, Error = ex.Message };
                }
              
            }

            try
            {
                await _operationRecordService.AddOperationRecord(OperationRecordType.Booking, booking.Id.ToString(), user, model.Identification, HttpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户 {Name}({Id})身份识别失败", user.UserName, user.Id);
            }

            //计算总数
            var tempCount = await _bookingUserRepository.CountAsync(s => s.BookingId == booking.Id);
            await _bookingRepository.GetAll().Where(s => s.Id == booking.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.BookingCount, b => tempCount));

            return new Result { Successful = true };
        }

        /// <summary>
        /// 获取用户是否已预约
        /// </summary>
        /// <param name="id">词条Id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingGameStateModel>> GetBookingGameState(long id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //通过Id获取文章
            var booking = await _bookingRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.EntryId == id);
            if (booking == null|| booking.Open == false)
            {
                return NotFound();
            }

            //建立视图模型
            var model = new BookingGameStateModel();

            model.IsBooking = await _bookingUserRepository.GetAll().AsNoTracking().AnyAsync(s => s.BookingId == booking.Id && s.ApplicationUserId == user.Id);

            return model;
        }


    }
}
