
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Gt3_server_csharp_aspnetcoremvc_bypass.Controllers.Sdk
{
    /*
     * sdk lib包，核心逻辑。
     *
     * @author liuquan@geetest.com
     */
    public class GeetestLib
    {
        // 公钥
        private readonly string geetest_id;

        // 私钥
        private readonly string geetest_key;

        // 返回数据的封装对象
        private readonly GeetestLibResult libResult;

        // 调试开关，是否输出调试日志
        private const bool IS_DEBUG = false;

        private const string API_URL = "http://api.geetest.com";

        private const string REGISTER_URL = "/register.php";

        private const string VALIDATE_URL = "/validate.php";

        private const string JSON_FORMAT = "1";

        private const bool NEW_CAPTCHA = true;

        private const int HTTP_TIMEOUT_DEFAULT = 5000; // 单位：毫秒

        public const string VERSION = "csharp-aspnetcoremvc:3.1.1";

        // 极验二次验证表单传参字段 chllenge
        public const string GEETEST_CHALLENGE = "geetest_challenge";

        // 极验二次验证表单传参字段 validate
        public const string GEETEST_VALIDATE = "geetest_validate";

        // 极验二次验证表单传参字段 seccode
        public const string GEETEST_SECCODE = "geetest_seccode";


        public GeetestLib(string geetest_id, string geetest_key)
        {
            this.geetest_id = geetest_id;
            this.geetest_key = geetest_key;
            libResult = new GeetestLibResult();
        }

        public void Gtlog(string message)
        {
            if (IS_DEBUG)
            {
                Console.WriteLine("Gtlog: " + message);
            }
        }

        // 验证初始化
        public GeetestLibResult Register(string digestmod, IDictionary<string, string> paramDict)
        {
            Gtlog($"Register(): 开始验证初始化, digestmod={digestmod}.");
            var origin_challenge = RequestRegister(paramDict);
            BuildRegisterResult(origin_challenge, digestmod);
            Gtlog($"Register(): 验证初始化, lib包返回信息={libResult}.");
            return libResult;
        }

        public GeetestLibResult LocalRegister()
        {
            BuildRegisterResult("", "");
            Gtlog($"LocalRegister(): bypass当前状态为fail，后续流程将进入宕机模式,={libResult}.");
            return libResult;
        }

        // 向极验发送验证初始化的请求，GET方式
        private string RequestRegister(IDictionary<string, string> paramDict)
        {
            paramDict.Add("gt", geetest_id);
            paramDict.Add("json_format", JSON_FORMAT);
            paramDict.Add("sdk", VERSION);
            var register_url = API_URL + REGISTER_URL;
            Gtlog($"RequestRegister(): 验证初始化, 向极验发送请求, url={register_url}, params={JsonSerializer.Serialize(paramDict)}.");
            string origin_challenge;
            try
            {
                var resBody = HttpGet(register_url, paramDict);
                Gtlog($"RequestRegister(): 验证初始化, 与极验网络交互正常, 返回body={resBody}.");
                var resDict = JsonSerializer.Deserialize<Dictionary<string, string>>(resBody);
                origin_challenge = resDict["challenge"];
            }
            catch (Exception e)
            {
                Gtlog("RequestRegister(): 验证初始化, 请求异常，后续流程走宕机模式, " + e);
                origin_challenge = "";
            }
            return origin_challenge;
        }

        // 构建验证初始化返回数据
        private void BuildRegisterResult(string origin_challenge, string digestmod)
        {
            // origin_challenge为空或者值为0代表失败
            if (string.IsNullOrWhiteSpace(origin_challenge) || "0".Equals(origin_challenge))
            {
                // 本地随机生成32位字符串
                var characters = "0123456789abcdefghijklmnopqrstuvwxyz";
                var randomStr = new StringBuilder();
                var rd = new Random();
                for (var i = 0; i < 32; i++)
                {
                    randomStr.Append(characters[rd.Next(characters.Length)]);
                }
                var challenge = randomStr.ToString();
                var data = new { success = 0, gt = geetest_id, challenge, new_captcha = NEW_CAPTCHA };
                libResult.SetAll(0, JsonSerializer.Serialize(data), "bypass当前状态为fail，后续流程走宕机模式");
            }
            else
            {
                string challenge;
                if ("md5".Equals(digestmod))
                {
                    challenge = Md5_encode(origin_challenge + geetest_key);
                }
                else if ("sha256".Equals(digestmod))
                {
                    challenge = Sha256_encode(origin_challenge + geetest_key);
                }
                else if ("hmac-sha256".Equals(digestmod))
                {
                    challenge = Hmac_sha256_encode(origin_challenge, geetest_key);
                }
                else
                {
                    challenge = Md5_encode(origin_challenge + geetest_key);
                }
                var data = new { success = 1, gt = geetest_id, challenge, new_captcha = NEW_CAPTCHA };
                libResult.SetAll(1, JsonSerializer.Serialize(data), "");
            }
        }

        // 正常流程下（即验证初始化成功），二次验证
        public GeetestLibResult SuccessValidate(string challenge, string validate, string seccode, IDictionary<string, string> paramDict)
        {
            Gtlog($"SuccessValidate(): 开始二次验证 正常模式, challenge={challenge}, validate={validate}, seccode={seccode}.");
            if (!CheckParam(challenge, validate, seccode))
            {
                libResult.SetAll(0, "", "正常模式，本地校验，参数challenge、validate、seccode不可为空");
            }
            else
            {
                var response_seccode = RequestValidate(challenge, seccode, paramDict);
                if (string.IsNullOrWhiteSpace(response_seccode))
                {
                    libResult.SetAll(0, "", "请求极验validate接口失败");
                }
                else if ("false".Equals(response_seccode))
                {
                    libResult.SetAll(0, "", "极验二次验证不通过");
                }
                else
                {
                    libResult.SetAll(1, "", "");
                }
            }
            Gtlog($"SuccessValidate(): 二次验证 正常模式, lib包返回信息={libResult}.");
            return libResult;
        }

        // 异常流程下（即验证初始化失败，宕机模式），二次验证
        // 注意：由于是宕机模式，初衷是保证验证业务不会中断正常业务，所以此处只作简单的参数校验，可自行设计逻辑。
        public GeetestLibResult FailValidate(string challenge, string validate, string seccode)
        {
            Gtlog($"FailValidate(): 开始二次验证 宕机模式, challenge={challenge}, validate={validate}, seccode={seccode}.");
            if (!CheckParam(challenge, validate, seccode))
            {
                libResult.SetAll(0, "", "宕机模式，本地校验，参数challenge、validate、seccode不可为空.");
            }
            else
            {
                libResult.SetAll(1, "", "");
            }
            Gtlog($"FailValidate(): 二次验证 宕机模式, lib包返回信息={libResult}.");
            return libResult;
        }

        // 向极验发送二次验证的请求，POST方式
        private string RequestValidate(string challenge, string seccode, IDictionary<string, string> paramDict)
        {
            paramDict.Add("seccode", seccode);
            paramDict.Add("json_format", JSON_FORMAT);
            paramDict.Add("challenge", challenge);
            paramDict.Add("sdk", VERSION);
            paramDict.Add("captchaid", geetest_id);
            var validate_url = API_URL + VALIDATE_URL;
            Gtlog($"RequestValidate(): 二次验证 正常模式, 向极验发送请求, url={validate_url}, params={JsonSerializer.Serialize(paramDict)}.");
            string response_seccode;
            try
            {
                var resBody = HttpPost(validate_url, paramDict);
                Gtlog($"RequestValidate(): 二次验证 正常模式, 与极验网络交互正常, 返回body={resBody}.");
                var resDict = JsonSerializer.Deserialize<Dictionary<string, string>>(resBody);
                response_seccode = resDict["seccode"];
            }
            catch (Exception e)
            {
                Gtlog("RequestValidate(): 二次验证 正常模式, 请求异常, " + e);
                response_seccode = "";
            }
            return response_seccode;
        }

        // 校验二次验证的三个参数，校验通过返回true，校验失败返回false
        private static bool CheckParam(string challenge, string validate, string seccode)
        {
            return !(string.IsNullOrWhiteSpace(challenge) || string.IsNullOrWhiteSpace(validate) || string.IsNullOrWhiteSpace(seccode));
        }

        // 发送GET请求，获取服务器返回结果
        private static string HttpGet(string url, IDictionary<string, string> paramDict)
        {
            Stream resStream = null;
            try
            {
                var paramStr = new StringBuilder();
                foreach (var item in paramDict)
                {
                    if (!(string.IsNullOrWhiteSpace(item.Key) || string.IsNullOrWhiteSpace(item.Value)))
                    {
                        paramStr.AppendFormat("&{0}={1}", HttpUtility.UrlEncode(item.Key, Encoding.UTF8), HttpUtility.UrlEncode(item.Value, Encoding.UTF8));
                    }

                }
                url = url + "?" + paramStr.ToString()[1..];
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.ReadWriteTimeout = HTTP_TIMEOUT_DEFAULT;
                req.Timeout = HTTP_TIMEOUT_DEFAULT;
                var res = (HttpWebResponse)req.GetResponse();
                resStream = res.GetResponseStream();
                var reader = new StreamReader(resStream, Encoding.GetEncoding("utf-8"));
                return reader.ReadToEnd();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (resStream != null)
                {
                    resStream.Close();
                }
            }
        }

        // 发送POST请求，获取服务器返回结果
        private static string HttpPost(string url, IDictionary<string, string> paramDict)
        {
            Stream reqStream = null;
            Stream resStream = null;
            try
            {
                var paramStr = new StringBuilder();
                foreach (var item in paramDict)
                {
                    if (!(string.IsNullOrWhiteSpace(item.Key) || string.IsNullOrWhiteSpace(item.Value)))
                    {
                        paramStr.AppendFormat("&{0}={1}", HttpUtility.UrlEncode(item.Key, Encoding.UTF8), HttpUtility.UrlEncode(item.Value, Encoding.UTF8));
                    }

                }
                var bytes = Encoding.UTF8.GetBytes(paramStr.ToString()[1..]);
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ReadWriteTimeout = HTTP_TIMEOUT_DEFAULT;
                req.Timeout = HTTP_TIMEOUT_DEFAULT;
                reqStream = req.GetRequestStream();
                reqStream.Write(bytes, 0, bytes.Length);
                var res = (HttpWebResponse)req.GetResponse();
                resStream = res.GetResponseStream();
                var reader = new StreamReader(resStream, Encoding.GetEncoding("utf-8"));
                return reader.ReadToEnd();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (reqStream != null)
                {
                    reqStream.Close();
                }
                if (resStream != null)
                {
                    resStream.Close();
                }
            }
        }

        // md5 加密
        private static string Md5_encode(string value)
        {
            using var md5 = MD5.Create();
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

        // sha256加密
        public string Sha256_encode(string value)
        {
            using var sha256 = SHA256.Create();
            var data = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

        // hmac-sha256 加密
        private static string Hmac_sha256_encode(string value, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var data = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

    }
}

