using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.Kanban.Components.Dialogs;
using CnGalWebSite.Kanban.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Chat
{
    public interface IChatService
    {
        bool IsOpen { get; set; }

        void Close();

        Task GetChatReply(GetKanbanReplyModel model);

        Task<List<ChatModel>> GetChatRecords(DeviceIdentificationModel Identification);

        void Init(ChatCard chatCard);

        void Clear();

        void Add(ChatModel model);

        void Show();
    }
}
