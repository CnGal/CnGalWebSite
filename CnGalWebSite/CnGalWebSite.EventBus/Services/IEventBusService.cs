using CnGalWebSite.EventBus.Models;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.EventBus.Services
{
    public interface IEventBusService
    {
        void SendQQMessage(QQMessageModel model);

        void RecieveQQMessage(Action<QQMessageModel> action);

        void SendQQGroupMessage(QQGroupMessageModel model);

        void RecieveQQGroupMessage(Action<QQGroupMessageModel> action);

        void SendRunTimedTask(RunTimedTaskModel model);

        void RecieveRunTimedTask(Action<RunTimedTaskModel> action);

        void CreateKanbanServer(Func<KanbanChatGPTSendModel, Task<KanbanChatGPTReceiveModel>> func);

        void InitRpcClient();

        Task<KanbanChatGPTReceiveModel> CallKanbanChatGPT(KanbanChatGPTSendModel model, CancellationToken cancellationToken = default);

        Task<SensitiveWordsResultModel> CallSensitiveWordsCheck(SensitiveWordsCheckModel model, CancellationToken cancellationToken = default);

        void CreateSensitiveWordsCheckServer(Func<SensitiveWordsCheckModel, Task<SensitiveWordsResultModel>> func);
    }
}
