using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using CnGalWebSite.DataModel.ViewModel.Space;
using Gt3_server_csharp_aspnetcoremvc_bypass.Controllers.Sdk;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    
    [Route("api/account/[action]")]
    [ApiController]
    [Authorize]
    public class AccountAPIController : ControllerBase
    {
        
        
        private readonly IAppHelper _appHelper;
        private readonly IRepository<UserOnlineInfor, long> _userOnlineInforRepository;
        private readonly IRepository<HistoryUser, int> _historyUserRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IUserService _userService;
        private readonly IOperationRecordService _operationRecordService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountAPIController> _logger;
        private readonly IQueryService _queryService;
        private readonly IRepository<UserCertification, long> _userCertificationRepository;
        private readonly IRepository<OperationRecord, long> _operationRecordRepository;


        public AccountAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository,  IAppHelper appHelper, IRepository<ApplicationUser, string> userRepository, IQueryService queryService, IRepository<UserCertification, long> userCertificationRepository,
        IRepository<HistoryUser, int> historyUserRepository, IUserService userService, IConfiguration configuration, ILogger<AccountAPIController> logger, IOperationRecordService operationRecordService, IRepository<OperationRecord, long> operationRecordRepository)
        {
            _appHelper = appHelper;
            _userOnlineInforRepository = userOnlineInforRepository;
            _historyUserRepository = historyUserRepository;
            _userRepository = userRepository;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
            _operationRecordService = operationRecordService;
            _queryService = queryService;
            _userCertificationRepository = userCertificationRepository;
            _operationRecordRepository = operationRecordRepository;
        }

        /// <summary>
        /// 极验初始化验证接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<GeetestCodeModel>> GetGeetestCode()
        {
            var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            /*
            必传参数
                digestmod 此版本sdk可支持md5、sha256、hmac-sha256，md5之外的算法需特殊配置的账号，联系极验客服
            自定义参数,可选择添加
                user_id user_id作为客户端用户的唯一标识，确定用户的唯一性；作用于提供进阶数据分析服务，可在register和validate接口传入，不传入也不影响验证服务的使用；若担心用户信息风险，可作预处理(如哈希处理)再提供到极验
                client_type 客户端类型，web：电脑上的浏览器；h5：手机上的浏览器，包括移动应用内完全内置的web_view；native：通过原生sdk植入app应用的方式；unknown：未知
                ip_address 客户端请求sdk服务器的ip地址
            */
            var gtLib = new GeetestLib(_configuration["GEETEST_ID"], _configuration["GEETEST_KEY"]);
            var userId = ip;
            var digestmod = "md5";
            IDictionary<string, string> paramDict = new Dictionary<string, string> { { "digestmod", digestmod }, { "user_id", userId }, { "client_type", "web" }, { "ip_address", "127.0.0.1" } };
            var bypass_cache = "success";
            GeetestLibResult result;
            if (bypass_cache == "success")
            {
                result = gtLib.Register(digestmod, paramDict);
            }
            else
            {
                result = gtLib.LocalRegister();
            }
            var obj = JObject.Parse(result.GetData());
            var model = new GeetestCodeModel
            {
                Challenge = obj["challenge"].ToString(),
                Success = obj["success"].ToString(),
                Gt = obj["gt"].ToString()
            };
            return await Task.FromResult(model);
        }

        /// <summary>
        /// 获取Ip地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetIp()
        {
            return await Task.FromResult(_operationRecordService.GetIp(HttpContext, null));
        }

        /// <summary>
        /// 当前用户在线 定期调用
        /// </summary>
        /// <remarks>
        /// 每次调用会增加用户在线时间
        /// 间隔超过十分钟的按十分钟计算
        /// 建议每十分钟调用一次
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<Result>> MakeUserOnlineAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user != null)
            {
                //判断上次登入时间是否在 10分钟之前
                if (user.LastOnlineTime.AddSeconds(60 * 10) < DateTime.Now.ToCstTime())
                {
                    //添加在线时间
                    user.OnlineTime += 60 * 10;
                }
                else
                {
                    user.OnlineTime += (long)((DateTime.Now.ToCstTime() - user.LastOnlineTime).TotalSeconds);
                }
                user.LastOnlineTime = DateTime.Now.ToCstTime();

                await _userRepository.UpdateAsync(user);

                //判断是否写入登入信息
                var tempDateTimeNow = DateTime.Now.ToCstTime();
                if (await _userOnlineInforRepository.GetAll().AnyAsync(s => s.Date.Date == tempDateTimeNow.Date && s.ApplicationUserId == user.Id) == false)
                {
                    await _userOnlineInforRepository.InsertAsync(new UserOnlineInfor
                    {
                        ApplicationUser = user,
                        ApplicationUserId = user.Id,
                        Date = DateTime.Now.ToCstTime().Date
                    });
                }

                return new Result { Successful = true, Error = user.Email };
            }
            else
            {
                return new Result { Successful = false };
            }

        }

        [HttpGet]   
        public async Task <ActionResult<UserInfoModel>> GetUserInfo()
        {
           var user=await _appHelper.GetAPICurrentUserAsync(HttpContext);
            return new UserInfoModel
            {
                Email = user.Email,
                Name = user.UserName,
                Id = user.Id
            };
        }

        [HttpPost]
        public async Task<QueryResultModel<UserOverviewModel>> ListUsers(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ApplicationUser, string>(_userRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || ( s.UserName.Contains(model.SearchText) || s.Email.Contains(model.SearchText) || s.PersonalSignature.Contains(model.SearchText)));

            return new QueryResultModel<UserOverviewModel>
            {
                Items = await items.Select(s => new UserOverviewModel
                {
                    Id = s.Id,
                    PersonalSignature = s.PersonalSignature,
                    CanComment=s.CanComment??false,
                    DisplayContributionValue=s.DisplayContributionValue,
                    DisplayIntegral = s.DisplayIntegral,
                    Email = s.Email,
                    LastOnlineTime = s.LastOnlineTime,
                    OnlineTime = s.OnlineTime,
                    RegistTime=s.RegistTime,
                    UserName = s.UserName,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<QueryResultModel<UserCertificationOverviewModel>> ListUserCertifications(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<UserCertification, long>(_userCertificationRepository.GetAll().AsSingleQuery().Include(s => s.ApplicationUser).Include(s=>s.Entry).Where(s=>s.ApplicationUser!=null&&s.Entry!=null), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.ApplicationUser.UserName.Contains(model.SearchText) || s.Entry.Name.Contains(model.SearchText) ));

            return new QueryResultModel<UserCertificationOverviewModel>
            {
                Items = await items.Select(s => new UserCertificationOverviewModel
                {
                    Id = s.Id,
                    UserName = s.ApplicationUser.UserName,
                    UserId = s.ApplicationUserId,
                    CertificationTime = s.CertificationTime,
                    EntryId = s.Entry.Id,
                    EntryName = s.Entry.Name,
                    EntryType=s.Entry.Type
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<QueryResultModel<OperationRecordOverviewModel>> ListOperationRecords(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<OperationRecord, long>(_operationRecordRepository.GetAll().AsSingleQuery().Include(s => s.ApplicationUser).Where(s => s.ApplicationUser != null), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.ApplicationUser.UserName.Contains(model.SearchText)));

            return new QueryResultModel<OperationRecordOverviewModel>
            {
                Items = await items.Select(s => new OperationRecordOverviewModel
                {
                    Id = s.Id,
                    UserName = s.ApplicationUser.UserName,
                    UserId = s.ApplicationUserId,
                    Cookie = s.Cookie,
                    Ip = s.Ip,
                    ObjectId = s.ObjectId,
                    Type = s.Type,
                    OperationTime = s.OperationTime,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<GetQQResultModel> GetQQ(GetQQModel model)
        {
            if (_configuration["JwtSecurityKey"]!=model.Token)
            {
                return new GetQQResultModel();
            }

            var user =await _userRepository.FirstOrDefaultAsync(s => s.Id == model.UserId);
            if(user == null)
            {
                return new GetQQResultModel();
            }

            return new GetQQResultModel
            {
                QQ = user.GroupQQ
            };
        }
    }
}
