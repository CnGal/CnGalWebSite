using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using Gt3_server_csharp_aspnetcoremvc_bypass.Controllers.Sdk;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Geetest
{
    public class GeetestService:IGeetestService
    {
        private readonly IConfiguration _configuration;

        public GeetestService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public GeetestCodeModel GetGeetestCode(ControllerBase controller)
        {
            var ip = controller.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = controller.Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
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

        public bool CheckRecaptcha(HumanMachineVerificationResult model)
        {
            return CheckRecaptcha(model.Challenge, model.Validate, model.Seccode);
        }

        public bool CheckRecaptcha(string challenge, string validate, string seccode)
        {
            try
            {
                GeetestLibResult result = null;
                IDictionary<string, string> paramDict = new Dictionary<string, string> { };
                var gtLib = new GeetestLib(_configuration["GEETEST_ID"], _configuration["GEETEST_KEY"]);
                result = gtLib.SuccessValidate(challenge, validate, seccode, paramDict);
                return result.GetStatus() == 1;
            }
            catch
            {
                return false;
            }
        }


    }
}
