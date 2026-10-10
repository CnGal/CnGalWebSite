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
using CnGalWebSite.DrawingBed.Configuration;
using Microsoft.Extensions.Options;

namespace CnGalWebSite.DrawingBed.Services
{
    public class UploadService : IUploadService
    {
        private readonly ILogger<UploadService> _logger;
        private readonly AliyunOssOptions _aliyunOptions;
        private readonly TencentCosOptions _tencentOptions;
        private readonly TucangCcOptions _tucangOptions;
        private readonly HttpClient _httpClient;

        private string _aliyunBucketName;

        private CosXml _tencentCosXml;
        private OssClient _aliyunOssClient;

        public UploadService(ILogger<UploadService> logger, IOptions<AliyunOssOptions> aliyunOptions,
            IOptions<TencentCosOptions> tencentOptions, IOptions<TucangCcOptions> tucangOptions, HttpClient httpClient)
        {
            _logger = logger;
            _aliyunOptions = aliyunOptions.Value;
            _tencentOptions = tencentOptions.Value;
            _tucangOptions = tucangOptions.Value;
            _httpClient = httpClient;

            InitTencentOSS();
            InitAliyunOSS();
        }

        private void InitTencentOSS()
        {
            //初始化 CosXmlConfig 
            string region = _tencentOptions.Region; //设置一个默认的存储桶地域
            CosXmlConfig config = new CosXmlConfig.Builder()
              .IsHttps(true)  //设置默认 HTTPS 请求
              .SetRegion(region)  //设置一个默认的存储桶地域
              .SetDebugLog(true)  //显示日志
              .Build();  //创建 CosXmlConfig 对象
            string secretId = _tencentOptions.SecretId; //用户的 SecretId，建议使用子账号密钥，授权遵循最小权限指引，降低使用风险。子账号密钥获取可参见 https://cloud.tencent.com/document/product/598/37140
            string secretKey = _tencentOptions.SecretKey; //用户的 SecretKey，建议使用子账号密钥，授权遵循最小权限指引，降低使用风险。子账号密钥获取可参见 https://cloud.tencent.com/document/product/598/37140
            long durationSecond = 600;  //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(
              secretId, secretKey, durationSecond);
            _tencentCosXml = new CosXmlServer(config, cosCredentialProvider);
        }

        private void InitAliyunOSS()
        {
            // yourEndpoint填写Bucket所在地域对应的Endpoint。以华东1（杭州）为例，Endpoint填写为https://oss-cn-hangzhou.aliyuncs.com
            var endpoint = _aliyunOptions.Endpoint;
            // 阿里云账号AccessKey拥有所有API的访问权限，风险很高。强烈建议您创建并使用RAM用户进行API访问或日常运维，请登录RAM控制台创建RAM用户
            var accessKeyId = _aliyunOptions.AccessKeyId;
            var accessKeySecret = _aliyunOptions.AccessKeySecret;
            // yourBucketName填写Bucket名称
            _aliyunBucketName = _aliyunOptions.BucketName;

            // 创建OSSClient实例
            _aliyunOssClient = new OssClient(endpoint, accessKeyId, accessKeySecret);
        }

        public async Task<string> UploadToAliyunOSS(string filePath, string shar1)
        {
            if (!string.Equals(Path.GetExtension(filePath), ".mp3", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("仅允许发布转码后的 MP3 音频");
            return await Task.Run(() =>
            {
                // 填写Object完整路径，完整路径中不能包含Bucket名称，例如exampledir/exampleobject.txt
                var objectName = $"{UploadFilePolicy.AudioObjectPrefix}{DateTime.UtcNow:yyyyMMdd}/{shar1}.mp3";
                try
                {
                    // 上传文件
                    var result = _aliyunOssClient.PutObject(_aliyunBucketName, objectName, filePath,
                        new ObjectMetadata { ContentType = "audio/mpeg", ContentDisposition = "inline" });
                    var url = _aliyunOptions.PublicBaseAddress + objectName;
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
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = UploadFilePolicy.GetImageContentType(extension);
            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();

            // 初始化 TransferManager
            TransferManager transferManager = new TransferManager(_tencentCosXml, transferConfig);

            var bucket = _tencentOptions.BucketName; //存储桶，格式：BucketName-APPID
            var cosPath = $"{UploadFilePolicy.ImageObjectPrefix}{DateTime.UtcNow:yyyyMMdd}/{shar1}{extension}";
            var srcPath = filePath;//本地文件绝对路径

            // 上传对象
            // 上传任务会将这些请求头同时用于普通 PUT 请求和分块上传初始化请求
            var request = new COSXML.Model.Object.PutObjectRequest(bucket, cosPath, srcPath);
            request.SetRequestHeader("Content-Type", contentType);
            request.SetRequestHeader("Content-Disposition", "inline");
            COSXMLUploadTask uploadTask = new COSXMLUploadTask(request);
            uploadTask.SetSrcPath(srcPath);

            try
            {
                COSXMLUploadTask.UploadTaskResult result = await
                transferManager.UploadAsync(uploadTask);

                var url = _tencentOptions.PublicBaseAddress + cosPath;
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
        public async Task<string> UploadToTucangCC(string filePath, string uploadName)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(File.OpenRead(filePath));

                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                var mimeType = UploadFilePolicy.GetImageContentType(extension);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);

                content.Add(
                    content: fileContent,
                    name: "file",
                    fileName: Path.GetFileNameWithoutExtension(uploadName) + extension);
                content.Add(new StringContent(_tucangOptions.ApiToken), "token");

                var response = await _httpClient.PostAsync(_tucangOptions.UploadUrl, content);

                var newUploadResults = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(newUploadResults);

                if (result["code"].ToObject<int>() == 200)
                {

                    var url = $"{_tucangOptions.PublicBaseAddress}{result["data"]["url"].ToObject<string>().Split('/').LastOrDefault()}";
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
