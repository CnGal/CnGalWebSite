using CnGalWebSite.EventBus.Models;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.EventBus.Services
{
    public class EventBusService:IEventBusService
    {
        private readonly IEventBus _eventBus;

        public EventBusService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

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
    }
}
