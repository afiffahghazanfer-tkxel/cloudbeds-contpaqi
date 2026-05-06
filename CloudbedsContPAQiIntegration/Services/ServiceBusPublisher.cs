using Azure.Messaging.ServiceBus;
using CloudbedsContPAQi.Shared.Models.Messages;
using System.Text.Json;

namespace CloudbedsContPAQi.WebhookReceiver.Services
{
    public interface IServiceBusPublisher
    {
        Task PublishAsync(FinancialEventMessage message);
    }

    public class ServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
    {
        private readonly ServiceBusSender _sender;
        private readonly ILogger<ServiceBusPublisher> _logger;

        public ServiceBusPublisher(ServiceBusClient client, IConfiguration config, ILogger<ServiceBusPublisher> logger)
        {
            var queueName = config["ServiceBus:RawEventsQueue"] ?? "cloudbeds-raw-events";
            _sender = client.CreateSender(queueName);
            _logger = logger;
        }

        public async Task PublishAsync(FinancialEventMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var sbMessage = new ServiceBusMessage(json)
            {
                MessageId = message.MessageId,
                Subject = message.EventType,           // makes messages easy to filter in Azure portal
                ContentType = "application/json"
            };

            await _sender.SendMessageAsync(sbMessage);
            _logger.LogDebug("Message {MessageId} sent to Service Bus.", message.MessageId);
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
        }
    }
}
