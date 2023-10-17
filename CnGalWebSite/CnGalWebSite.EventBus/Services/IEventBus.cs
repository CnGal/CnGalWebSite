using RabbitMQ.Client.Events;

namespace CnGalWebSite.EventBus.Services
{
    public interface IEventBus
    {
        void SendMessage<T>(string queue, T message);

        void SubscribeMessages<T>(string queue, Action<T> action);
    }
}
