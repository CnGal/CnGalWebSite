using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using CnGalWebSite.APIServer.DataReositories;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using CnGalWebSite.Extensions;
using static IdentityServer4.Models.IdentityResources;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Messages;
using CnGalWebSite.IdentityServer.Models.ViewModels.Messages;
using CnGalWebSite.IdentityServer.Data;

namespace CnGalWebSite.IdentityServer.Services.Messages
{
    public class MessageService : IMessageService
    {
        private readonly IRepository<ApplicationDbContext, SendRecord, long> _sendRecordRepository;
        private readonly IEmailService _EmailService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IRepository<ApplicationDbContext, SendRecord, long> sendRecordRepository, IEmailService EmailService, IViewRenderService viewRenderService, IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<MessageService> logger)
        {
            _sendRecordRepository = sendRecordRepository;
            _EmailService = EmailService;
            _viewRenderService = viewRenderService;
            _configuration = configuration;
            _clientFactory = clientFactory;
            _logger = logger;
        }


        public async Task<bool> SendVerificationEmailAsync(int code, string email, ApplicationUser user, VerificationCodeType type)
        {
            //检查是否超过上限
            if (await CheckSendCount(email, 10, 10))
            {
                return false;
            }

            var htmlContent = _viewRenderService.Render("~/Templates/VerificationMsgView.cshtml", new VerificationMsgViewModel
            {
                UserName = user.UserName,
                Code = code,
                Purpose = type.GetDisplayName()
            });
            _EmailService.Send(email, "验证你的CnGal资料站账号", htmlContent, true);

            //记录发送
            await _sendRecordRepository.InsertAsync(new SendRecord
            {
                Address = email,
                ApplicationUserId = user.Id,
                Purpose = type.ToPurpose(),
                Time = DateTime.UtcNow,
                Type = SendRecordType.Email
            });

            return true;
        }


        public async Task<bool> CheckSendCount(string address, int limit, int duration)
        {
            var timeNow = DateTime.UtcNow;
            var count = await _sendRecordRepository.GetAll().CountAsync(s => s.Address == address && s.Time.AddMinutes(duration) > timeNow);
            return count >= limit;
        }

        public async Task<bool> SendVerificationSMSAsync(int code, string phoneNumber, ApplicationUser user, VerificationCodeType type)
        {
            if(phoneNumber.StartsWith("+")==false)
            {
                phoneNumber = "86" + phoneNumber;
            }
            else
            {
                phoneNumber = phoneNumber.TrimStart('+');
            }

            //检查是否超过上限
            if (await CheckSendCount(phoneNumber, 10, 10))
            {
                return false;
            }

            var client = CreateClient(_configuration["AliyunAccessKeyId"], _configuration["AliyunAccessKeySecret"]);
            var sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
            {

                PhoneNumbers = phoneNumber,
                SignName = phoneNumber.StartsWith("86") ? "cngal":"CnGal资料站",
                TemplateCode = _configuration["SMS_"+(phoneNumber.StartsWith("86")?"": "Globe_") + type.ToString()] ?? throw new Exception("未找到匹配的SMS模板"),
                TemplateParam = "{ 'code':'" + code + "'}",
            };

            // 复制代码运行请自行打印 API 的返回值
            var result = await client.SendSmsAsync(sendSmsRequest );
            if (result.Body.Code != "OK")
            {
                _logger.LogError("发送短信失败：{phone} {msg}", phoneNumber, result.Body.Message);
                throw new Exception(result.Body.Message);
            }

            //记录发送
            _logger.LogInformation("发送短信成功：{phone}", phoneNumber);

            await _sendRecordRepository.InsertAsync(new SendRecord
            {
                Address = phoneNumber,
                ApplicationUserId = user.Id,
                Purpose = type.ToPurpose(),
                Time = DateTime.UtcNow,
                Type = SendRecordType.SMS
            });

            return true;
        }

        /// <summary>
        /// 初始化账号Client
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="accessKeySecret"></param>
        /// <returns></returns>
        public static AlibabaCloud.SDK.Dysmsapi20170525.Client CreateClient(string accessKeyId, string accessKeySecret)
        {
            var config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // 您的AccessKey ID
                AccessKeyId = accessKeyId,
                // 您的AccessKey Secret
                AccessKeySecret = accessKeySecret,
                // 访问的域名
                Endpoint = "dysmsapi.aliyuncs.com"
            };
            return new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
        }


    }
}
