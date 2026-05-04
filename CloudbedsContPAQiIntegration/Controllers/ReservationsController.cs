using CloudbedsContPAQiIntegration.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloudbedsContPAQiIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ICloudbedsReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            ICloudbedsReservationService reservationService,
            ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// Get reservations from Cloudbeds for a given date range.
        /// Defaults to yesterday → today if no dates provided.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReservations(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _reservationService.GetReservationsAsync(from, to);
            return Ok(result);
        }

        /// <summary>
        /// Get a single reservation by ID.
        /// </summary>
        [HttpGet("{reservationId}")]
        public async Task<IActionResult> GetReservationById(string reservationId)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(reservationId);

            if (reservation is null)
                return NotFound(new { message = $"Reservation {reservationId} not found." });

            return Ok(reservation);
        }
    }
}
