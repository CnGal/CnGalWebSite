using CnGalWebSite.Core.Services;
using CnGalWebSite.EventBus.Services;

namespace CnGalWebSite.ProjectSite.API.Services.Notices
{
    public class NoticeService : INoticeService
    {
        private readonly IEventBusService _eventBusService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NoticeService> _logger;

        public NoticeService(IEventBusService eventBusService, IConfiguration configuration, ILogger<NoticeService> logger)
        {
            _eventBusService = eventBusService;
            _configuration = configuration;
            _logger = logger;
        }

        public void PutNotice(string notice)
        {
            try
            {
                _eventBusService.SendQQGroupMessage(new EventBus.Models.QQGroupMessageModel
                {
                    GroupId = long.Parse(_configuration["NoticeQQGroup"]),
                    Message = notice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送通知失败");
            }
        }

    }
}
