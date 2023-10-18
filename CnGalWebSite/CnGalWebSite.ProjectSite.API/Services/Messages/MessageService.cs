using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.EventBus.Services;
using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Messages;

namespace CnGalWebSite.ProjectSite.API.Services.Messages
{
    public class MessageService : IMessageService
    {
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IEventBusService _eventBusService;
        private readonly IHttpService _httpService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IRepository<Message, long> messageRepository, IEventBusService eventBusService, IHttpService httpService, IConfiguration configuration, ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository;
            _eventBusService = eventBusService;
            _configuration = configuration;
            _logger = logger;
            _httpService = httpService;
        }

        public async Task PutMessage(PutMessageModel model)
        {
            if (await _messageRepository.AnyAsync(s => s.Text == model.Text))
            {
                return;
            }

            await _messageRepository.InsertAsync(new Message
            {
                CreateTime = DateTime.Now.ToCstTime(),
                PageId = model.PageId,
                PageType = model.PageType,
                Text = model.Text,
                Type = model.Type,
                UpdateTime = DateTime.Now.ToCstTime(),
                UserId = model.UserId,
            });

            try
            {
                var result = await _httpService.PostAsync<GetQQModel, GetQQResultModel>($"{_configuration["CnGalAPI"]}api/account/GetQQ", new GetQQModel
                {
                    Token = _configuration["JwtSecurityKey"],
                    UserId = model.UserId
                });
                if (result.QQ != 0)
                {
                    var link = model.PageType switch
                    {
                        PageType.Project => $"https://www.cngal.org.cn/project/{model.PageId}",
                        _ => "https://www.cngal.org.cn"
                    };
                    //向用户推送消息
                    _eventBusService.SendQQMessage(new EventBus.Models.QQMessageModel
                    {
                        Message =$"【CnGal企划站】\n{model.Text}\n{link}",
                        QQ = result.QQ
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "获取QQ并推送消息失败，用户Id：{id}",model.UserId);
            }
        }
    }
}
