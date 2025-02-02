using RabbitMQ.Client.Events;

namespace CnGalWebSite.EventBus.Services
{
    public interface IEventBus
    {
        void SendMessage<T>(string queue, T message);

        void SubscribeMessages<T>(string queue, Action<T> action);

        void CreateRpcServer<TInput, TOutput>(string queue, Func<TInput, Task<TOutput>> func);

        void CreateRpcClient();

        Task<TOutput> CallRpcAsync<TInput, TOutput>(string queue, TInput input, CancellationToken cancellationToken = default);
    }
}
