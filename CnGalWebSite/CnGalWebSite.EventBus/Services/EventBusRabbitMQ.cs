using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace CnGalWebSite.EventBus.Services
{
    public class EventBusRabbitMQ:IEventBus,IDisposable
    {
        private readonly IConfiguration _configuration;

        private IConnection _connection;
        private IModel _channel;

        public EventBusRabbitMQ(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Init()
        {
            if(_channel == null)
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["EventBus_HostName"],
                    Port = int.Parse(_configuration["EventBus_Port"]),
                    UserName = _configuration["EventBus_UserName"],
                    Password = _configuration["EventBus_Password"],
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
        }

        public void SendMessage<T>(string queue,T message)
        {
            Init();
            _channel.QueueDeclare(
                queue: queue,   // 队列名称
                durable: true,     // 是否持久化，true持久化，队列会保存磁盘，服务器重启时可以保证不丢失相关信息
                exclusive: false,   // 是否排他，如果一个队列声明为排他队列，该队列仅对时候次声明它的连接可见，并在连接断开时自动删除
                autoDelete: false,  // 是否自动删除，自动删除的前提是：至少有一个消费者连接到这个队列，之后所有与这个队列连接的消费者都断开时，才会自动删除
                arguments: null     // 设置队列的其他参数
            );

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            _channel.BasicPublish(exchange: "", routingKey: queue, body: body);
        }

        public void SubscribeMessages<T>(string queue, Action<T> action)
        {
            Init();
            _channel.QueueDeclare(
                queue: queue,   // 队列名称
                durable: true,     // 是否持久化，true持久化，队列会保存磁盘，服务器重启时可以保证不丢失相关信息
                exclusive: false,   // 是否排他，如果一个队列声明为排他队列，该队列仅对时候次声明它的连接可见，并在连接断开时自动删除
                autoDelete: false,  // 是否自动删除，自动删除的前提是：至少有一个消费者连接到这个队列，之后所有与这个队列连接的消费者都断开时，才会自动删除
                arguments: null     // 设置队列的其他参数
            );

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var obj= JsonSerializer.Deserialize<T>(message);
                action(obj);
            };
            _channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _connection= null;
            _channel?.Dispose();
            _channel= null;
        }
    }
}
