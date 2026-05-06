namespace CloudbedsContPAQi.Shared.Models.Webhooks
{
    /// <summary>
    /// Base webhook payload received from Cloudbeds.
    /// Every event shares this envelope structure.
    /// </summary>
    public class CloudbedsWebhookPayload
    {
        public string PropertyId { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;   // e.g. "transaction/created"
        public string Version { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public WebhookData? Data { get; set; }
    }

    public class WebhookData
    {
        // Shared across events
        public string? ReservationId { get; set; }

        // transaction/created
        public string? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionType { get; set; }  // charge, payment, adjustment

        // reservation/status_changed
        public string? Status { get; set; }           // checked_out, checked_in, etc.

        // payment/voided
        public string? PaymentId { get; set; }
    }
}
