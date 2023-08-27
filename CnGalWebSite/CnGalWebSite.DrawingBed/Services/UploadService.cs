using COSXML.Auth;
using COSXML;
using COSXML.Common;
using COSXML.Model.Service;
using COSXML.Model.Tag;
using COSXML.Transfer;
using System.Security.AccessControl;
using System.Security.Policy;
using Aliyun.OSS;
using Newtonsoft.Json.Linq;

namespace CnGalWebSite.DrawingBed.Services
{
    public class UploadService : IUploadService
    {
        private readonly ILogger<UploadService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private string _aliyunBucketName;

        private CosXml _tencentCosXml;
        private OssClient _aliyunOssClient;

        public UploadService(ILogger<UploadService> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;

            InitTencentOSS();
            InitAliyunOSS();
        }

        private void InitTencentOSS()
        {
            //初始化 CosXmlConfig 
            string region = _configuration["Tencent_COS_REGION"]; //设置一个默认的存储桶地域
            CosXmlConfig config = new CosXmlConfig.Builder()
              .IsHttps(true)  //设置默认 HTTPS 请求
              .SetRegion(region)  //设置一个默认的存储桶地域
              .SetDebugLog(true)  //显示日志
              .Build();  //创建 CosXmlConfig 对象
            string secretId = _configuration["Tencent_SECRET_ID"]; //用户的 SecretId，建议使用子账号密钥，授权遵循最小权限指引，降低使用风险。子账号密钥获取可参见 https://cloud.tencent.com/document/product/598/37140
            string secretKey = _configuration["Tencent_SECRET_KEY"]; //用户的 SecretKey，建议使用子账号密钥，授权遵循最小权限指引，降低使用风险。子账号密钥获取可参见 https://cloud.tencent.com/document/product/598/37140
            long durationSecond = 600;  //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(
              secretId, secretKey, durationSecond);
            _tencentCosXml = new CosXmlServer(config, cosCredentialProvider);
        }

        private void InitAliyunOSS()
        {
            // yourEndpoint填写Bucket所在地域对应的Endpoint。以华东1（杭州）为例，Endpoint填写为https://oss-cn-hangzhou.aliyuncs.com
            var endpoint = _configuration["OSSEndpoint"];
            // 阿里云账号AccessKey拥有所有API的访问权限，风险很高。强烈建议您创建并使用RAM用户进行API访问或日常运维，请登录RAM控制台创建RAM用户
            var accessKeyId = _configuration["OSSAccessKeyId"];
            var accessKeySecret = _configuration["OSSAccessKeySecret"];
            // yourBucketName填写Bucket名称
            _aliyunBucketName = _configuration["OSSBucketName"];

            // 创建OSSClient实例
            _aliyunOssClient = new OssClient(endpoint, accessKeyId, accessKeySecret);
        }

        public async Task<string> UploadToAliyunOSS(string filePath, string shar1)
        {
            return await Task.Run(() =>
            {
                // 填写Object完整路径，完整路径中不能包含Bucket名称，例如exampledir/exampleobject.txt
                var objectName = $"audio/upload/{DateTime.UtcNow:yyyyMMdd}/{shar1}.mp3";
                try
                {
                    // 上传文件
                    var result = _aliyunOssClient.PutObject(_aliyunBucketName, objectName, filePath);
                    var url = _configuration["AudioUrl"] + objectName;
                    _logger.LogInformation("成功上传音频到OSS：{url}", url);
                    return url;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "上传音频到OSS失败：{filePath}", filePath);
                    throw;
                }
            });
        }

        public async Task<string> UploadToTencentOSS(string filePath, string shar1)
        {
            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();

            // 初始化 TransferManager
            TransferManager transferManager = new TransferManager(_tencentCosXml, transferConfig);

            var bucket = _configuration["Tencent_BucketName"]; //存储桶，格式：BucketName-APPID
            var cosPath = $"images/upload/{DateTime.UtcNow:yyyyMMdd}/{shar1}.{filePath.Split('.').LastOrDefault() ?? "png"}"; //对象在存储桶中的位置标识符，即称对象键
            var srcPath = filePath;//本地文件绝对路径

            // 上传对象
            COSXMLUploadTask uploadTask = new COSXMLUploadTask(bucket, cosPath);
            uploadTask.SetSrcPath(srcPath);

            try
            {
                COSXMLUploadTask.UploadTaskResult result = await
                transferManager.UploadAsync(uploadTask);

                var url = _configuration["ImagesUrl"] + cosPath;
                _logger.LogInformation("成功上传图片到OSS：{url}", url);
                return url;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传图片到OSS失败：{filePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// 转存图片到公共图床
        /// </summary>
        /// <returns></returns>
        public async Task<string> UploadToTucangCC(string filePath)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(File.OpenRead(filePath));

                content.Add(
                    content: fileContent,
                    name: "file",
                    fileName: "test.png");
                content.Add(new StringContent(_configuration["TucangCCAPIToken"]), "token");

                var response = await _httpClient.PostAsync(_configuration["TucangCCAPIUrl"], content);

                var newUploadResults = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(newUploadResults);

                if (result["code"].ToObject<int>() == 200)
                {

                    var url= $"{_configuration["CustomTucangCCUrl"]}{result["data"]["url"].ToObject<string>().Split('/').LastOrDefault()}";
                    await _httpClient.GetAsync(url);
                    _logger.LogInformation("成功上传图片到TucangCC：{url}", url);
                    return url;
                }
                else
                {
                    _logger.LogError("转存图片失败，接口返回代码：{code}，消息：{msg}，图片：{filePath}", result["code"].ToObject<int>(), result["msg"].ToObject<string>(), filePath);
                    throw new Exception("转存图片失败");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "转存图片失败：{filePath}", filePath);
                throw;
            }
        }
    }
}
