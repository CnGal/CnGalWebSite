using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Admin;
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

namespace CnGalWebSite.APIServer.Controllers
{
    [AllowAnonymous]
    [Route("api/account/[action]")]
    [ApiController]
    public class AccountAPIController : ControllerBase
    {
        
        
        private readonly IAppHelper _appHelper;
        private readonly IRepository<UserOnlineInfor, long> _userOnlineInforRepository;
        private readonly IRepository<HistoryUser, int> _historyUserRepository;
        private readonly IRepository<ApplicationUser, int> _userRepository;
        private readonly IUserService _userService;
        private readonly IOperationRecordService _operationRecordService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountAPIController> _logger;


        public AccountAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository,  IAppHelper appHelper, IRepository<ApplicationUser, int> userRepository,
            IRepository<HistoryUser, int> historyUserRepository, IUserService userService, IConfiguration configuration, ILogger<AccountAPIController> logger, IOperationRecordService operationRecordService)
        {
            _appHelper = appHelper;
            _userOnlineInforRepository = userOnlineInforRepository;
            _historyUserRepository = historyUserRepository;
            _userRepository = userRepository;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
            _operationRecordService = operationRecordService;
        }

        /// <summary>
        /// 极验初始化验证接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
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
            return model;
        }

        /// <summary>
        /// 获取Ip地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<string>> GetIp()
        {
            return _operationRecordService.GetIp(HttpContext, null);
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
    }
}
