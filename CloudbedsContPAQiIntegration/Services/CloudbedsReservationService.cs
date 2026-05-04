using CloudbedsContPAQiIntegration.Configuration;
using CloudbedsContPAQiIntegration.Models.Cloudbeds;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CloudbedsContPAQiIntegration.Services
{
    public interface ICloudbedsReservationService
    {
        Task<ReservationListResponse> GetReservationsAsync(DateTime? from = null, DateTime? to = null);
        Task<Reservation?> GetReservationByIdAsync(string reservationId);
    }

    public class CloudbedsReservationService : ICloudbedsReservationService
    {
        private readonly HttpClient _httpClient;
        private readonly CloudbedsSettings _settings;
        private readonly ILogger<CloudbedsReservationService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CloudbedsReservationService(
            HttpClient httpClient,
            IOptions<CloudbedsSettings> settings,
            ILogger<CloudbedsReservationService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            // Attach API key to every request
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        public async Task<ReservationListResponse> GetReservationsAsync(
            DateTime? from = null,
            DateTime? to = null)
        {
            try
            {
                var query = BuildReservationQuery(from, to);
                var url = $"getReservations?{query}";

                _logger.LogInformation("Fetching reservations from Cloudbeds. URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ReservationListResponse>(content, _jsonOptions);

                _logger.LogInformation("Fetched {Count} reservations.", result?.Count ?? 0);

                return result ?? new ReservationListResponse();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching reservations from Cloudbeds.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching reservations.");
                throw;
            }
        }

        public async Task<Reservation?> GetReservationByIdAsync(string reservationId)
        {
            try
            {
                var url = $"getReservations?reservationID={reservationId}&propertyID={_settings.PropertyId}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ReservationListResponse>(content, _jsonOptions);

                return result?.Data.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reservation {ReservationId}.", reservationId);
                throw;
            }
        }

        private string BuildReservationQuery(DateTime? from, DateTime? to)
        {
            var dateFrom = (from ?? DateTime.Today.AddDays(-1)).ToString("yyyy-MM-dd");
            var dateTo = (to ?? DateTime.Today).ToString("yyyy-MM-dd");

            return $"propertyID={_settings.PropertyId}" +
                   $"&checkInFrom={dateFrom}" +
                   $"&checkInTo={dateTo}" +
                   $"&pageSize=100";
        }
    }
}
