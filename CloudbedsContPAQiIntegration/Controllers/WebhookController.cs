using CloudbedsContPAQi.Shared.Constants;
using CloudbedsContPAQi.Shared.Models.Messages;
using CloudbedsContPAQi.Shared.Models.Webhooks;
using CloudbedsContPAQi.WebhookReceiver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CloudbedsContPAQi.WebhookReceiver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IServiceBusPublisher _serviceBusPublisher;
        private readonly ILogger<WebhookController> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // Financial events we care about — everything else is dropped immediately
        private static readonly HashSet<string> _financialEvents = new()
        {
            CloudbedsWebhookEvents.TransactionCreated,
            CloudbedsWebhookEvents.ReservationStatusChanged,
            CloudbedsWebhookEvents.PaymentVoided
        };

        public WebhookController(
            IServiceBusPublisher serviceBusPublisher,
            ILogger<WebhookController> logger)
        {
            _serviceBusPublisher = serviceBusPublisher;
            _logger = logger;
        }

        /// <summary>
        /// Receives ALL Cloudbeds webhook events.
        /// Returns 200 immediately — Cloudbeds requires response within 5 seconds.
        /// Only financial events are enqueued; everything else is silently dropped.
        /// </summary>
        [HttpPost("cloudbeds")]
        public async Task<IActionResult> ReceiveCloudbeds([FromBody] JsonElement rawPayload)
        {
            var rawJson = rawPayload.GetRawText();

            try
            {
                var payload = JsonSerializer.Deserialize<CloudbedsWebhookPayload>(rawJson, _jsonOptions);

                if (payload == null)
                {
                    _logger.LogWarning("Received null or unparseable webhook payload.");
                    return Ok(); // Still return 200 — don't let Cloudbeds retry
                }

                _logger.LogInformation("Webhook received: {Event} for property {PropertyId}",
                    payload.Event, payload.PropertyId);

                // Drop non-financial events immediately
                if (!_financialEvents.Contains(payload.Event))
                {
                    _logger.LogDebug("Ignoring non-financial event: {Event}", payload.Event);
                    return Ok();
                }

                // For status_changed, only process checkouts
                if (payload.Event == CloudbedsWebhookEvents.ReservationStatusChanged
                    && payload.Data?.Status != "checked_out")
                {
                    _logger.LogDebug("Ignoring status_changed with status: {Status}", payload.Data?.Status);
                    return Ok();
                }

                // Build the message for Service Bus
                var message = new FinancialEventMessage
                {
                    EventType = payload.Event,
                    PropertyId = payload.PropertyId,
                    ReservationId = payload.Data?.ReservationId,
                    TransactionId = payload.Data?.TransactionId,
                    PaymentId = payload.Data?.PaymentId,
                    Amount = payload.Data?.Amount,
                    TransactionType = payload.Data?.TransactionType,
                    ReservationStatus = payload.Data?.Status,
                    RawPayload = rawJson
                };

                // Enqueue — fire and forget from Cloudbeds' perspective
                await _serviceBusPublisher.PublishAsync(message);

                _logger.LogInformation(
                    "Enqueued financial event {EventType} | ReservationId: {ReservationId} | MessageId: {MessageId}",
                    message.EventType, message.ReservationId, message.MessageId);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log but still return 200 — we don't want Cloudbeds to retry
                // Failed messages are handled by our own retry/dead-letter logic
                _logger.LogError(ex, "Error processing webhook. RawPayload: {Raw}", rawJson);
                return Ok();
            }
        }
    }
}
