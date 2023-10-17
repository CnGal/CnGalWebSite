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
    }
}
