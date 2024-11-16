using RabbitMQ.Client.Events;

namespace CnGalWebSite.EventBus.Services
{
    public interface IEventBus
    {
        Task SendMessage<T>(string queue, T message);

        Task SubscribeMessages<T>(string queue, Action<T> action);

        Task CreateRpcServer<TInput, TOutput>(string queue, Func<TInput, Task<TOutput>> func);

        Task CreateRpcClient();

        Task<TOutput> CallRpcAsync<TInput, TOutput>(string queue, TInput input, CancellationToken cancellationToken = default);
    }
}
