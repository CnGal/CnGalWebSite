using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Messages;

namespace CnGalWebSite.ProjectSite.API.Services.Messages
{
    public class MessageService:IMessageService
    {
        private readonly IRepository<Message, long> _messageRepository;

        public MessageService(IRepository<Message, long> messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task PutMessage(PutMessageModel model)
        {
            if(await _messageRepository.AnyAsync(s=>s.Text==model.Text))
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
        }
    }
}
