using CnGalWebSite.Core.Services;
using CnGalWebSite.EventBus.Services;

namespace CnGalWebSite.ProjectSite.API.Services.Notices
{
    public class NoticeService:INoticeService
    {
        private readonly IEventBusService _eventBusService;
        private readonly IConfiguration _configuration;

        public NoticeService(IEventBusService eventBusService, IConfiguration configuration)
        {
            _eventBusService = eventBusService;
            _configuration = configuration;
        }

        public void PutNotice(string notice)
        {
            _eventBusService.SendQQGroupMessage(new EventBus.Models.QQGroupMessageModel
            {
                GroupId = long.Parse(_configuration["NoticeQQGroup"]),
                Message = notice
            });
        }

    }
}
