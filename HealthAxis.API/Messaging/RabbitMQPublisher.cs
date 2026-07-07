using System.Text;
using System.Text.Json;
using HealthAxis.API.Events;
using RabbitMQ.Client;

namespace HealthAxis.API.Messaging
{
    public class RabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _queueName;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            var rabbitConfig = configuration.GetSection("RabbitMQ");

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["HostName"],
                Port = int.Parse(rabbitConfig["Port"]!),
                UserName = rabbitConfig["UserName"],
                Password = rabbitConfig["Password"],
                VirtualHost = rabbitConfig["VirtualHost"]
            };

            _queueName = rabbitConfig["AppointmentQueue"]!;

            // Create connection
            _connection = factory
                .CreateConnectionAsync()
                .GetAwaiter()
                .GetResult();

            // Create channel
            _channel = _connection
                .CreateChannelAsync()
                .GetAwaiter()
                .GetResult();

            // Create queue if it doesn't exist
            _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null)
                .GetAwaiter()
                .GetResult();
        }

        public async Task PublishAsync(AppointmentEvent appointmentEvent)
        {
            var message = JsonSerializer.Serialize(appointmentEvent);

            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                mandatory: false,
                basicProperties: properties,
                body: body);
        }

        public void Dispose()
        {
            _channel?.CloseAsync()
                .GetAwaiter()
                .GetResult();

            _connection?.CloseAsync()
                .GetAwaiter()
                .GetResult();
        }
    }
}