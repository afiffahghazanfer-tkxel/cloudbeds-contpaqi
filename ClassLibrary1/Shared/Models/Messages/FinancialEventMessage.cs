namespace CloudbedsContPAQi.Shared.Models.Messages
{
    /// <summary>
    /// The message we drop on Azure Service Bus after receiving a webhook.
    /// Contains only what the Transform worker needs — no noise.
    /// </summary>
    public class FinancialEventMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } = string.Empty;   // maps to CloudbedsWebhookEvents constants
        public string PropertyId { get; set; } = string.Empty;
        public string? ReservationId { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentId { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionType { get; set; }
        public string? ReservationStatus { get; set; }          // only for status_changed events
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public string RawPayload { get; set; } = string.Empty;  // always keep original for debugging
    }
}
