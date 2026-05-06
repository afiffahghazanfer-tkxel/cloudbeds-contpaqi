namespace CloudbedsContPAQi.Shared.Constants
{
    /// <summary>
    /// Only the financial webhook events we care about.
    /// All other Cloudbeds events (guest/created, housekeeping, etc.) are ignored.
    /// </summary>
    public static class CloudbedsWebhookEvents
    {
        // A charge, payment, or adjustment was posted to a folio
        public const string TransactionCreated = "transaction/created";

        // Reservation status changed — we only process "checked_out"
        public const string ReservationStatusChanged = "reservation/status_changed";

        // A payment was voided — needs reversal in ContPAQi
        public const string PaymentVoided = "payment/voided";
    }
}
