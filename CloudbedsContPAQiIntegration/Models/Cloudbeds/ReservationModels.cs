namespace CloudbedsContPAQiIntegration.Models.Cloudbeds
{
    public class ReservationListResponse
    {
        public bool Success { get; set; }
        public List<Reservation> Data { get; set; } = new();
        public int Total { get; set; }
        public int Count { get; set; }
    }

    public class Reservation
    {
        public string ReservationId { get; set; } = string.Empty;
        public string PropertyId { get; set; } = string.Empty;
        public string GuestId { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CheckIn { get; set; } = string.Empty;
        public string CheckOut { get; set; } = string.Empty;
        public decimal BalanceDue { get; set; }
        public decimal GrandTotal { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
