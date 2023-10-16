using CnGalWebSite.ProjectSite.Models.ViewModels.Messages;

namespace CnGalWebSite.ProjectSite.API.Services.Messages
{
    public interface IMessageService
    {
        Task PutMessage(PutMessageModel model);
    }
}
