using CnGalWebSite.EventBus.Models;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.EventBus.Services
{
    public class EventBusService(IEventBus eventBus) : IEventBusService
    {
        private readonly IEventBus _eventBus = eventBus;

        private bool _rpcClientInited;

        public void SendQQMessage(QQMessageModel model)
        {
            _eventBus.SendMessage("qq", model);
        }

        public void RecieveQQMessage(Action<QQMessageModel> action)
        {
            _eventBus.SubscribeMessages("qq", action);
        }

        public void SendQQGroupMessage(QQGroupMessageModel model)
        {
            _eventBus.SendMessage("qq_group", model);
        }

        public void RecieveQQGroupMessage(Action<QQGroupMessageModel> action)
        {
            _eventBus.SubscribeMessages("qq_group", action);
        }

        public void SendRunTimedTask(RunTimedTaskModel model)
        {
            _eventBus.SendMessage("timed_task", model);
        }

        public void RecieveRunTimedTask(Action<RunTimedTaskModel> action)
        {
            _eventBus.SubscribeMessages("timed_task", action);
        }

        public void CreateKanbanServer(Func<KanbanChatGPTSendModel, Task<KanbanChatGPTReceiveModel>> func)
        {
            _eventBus.CreateRpcServer("kanban_chatgpt", func);
        }

        public void CreateSensitiveWordsCheckServer(Func<SensitiveWordsCheckModel, Task<SensitiveWordsResultModel>> func)
        {
            _eventBus.CreateRpcServer("sensitive_words_check", func);
        }

        /// <summary>
        /// 初始化RPC客户端 只调用一次
        /// </summary>
        public void InitRpcClient()
        {
            if (_rpcClientInited)
            {
                return;
            }

            _eventBus.CreateRpcClient();
        }

        public async Task<KanbanChatGPTReceiveModel> CallKanbanChatGPT(KanbanChatGPTSendModel model, CancellationToken cancellationToken = default)
        {
            return await _eventBus.CallRpcAsync<KanbanChatGPTSendModel, KanbanChatGPTReceiveModel>("kanban_chatgpt", model, cancellationToken);
        }

        public async Task<SensitiveWordsResultModel> CallSensitiveWordsCheck(SensitiveWordsCheckModel model, CancellationToken cancellationToken = default)
        {
            return await _eventBus.CallRpcAsync<SensitiveWordsCheckModel, SensitiveWordsResultModel>("sensitive_words_check", model, cancellationToken);
        }

    }
}
