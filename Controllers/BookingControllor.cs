using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SessionService.Repository.Interfaces;

namespace SessionService.Controllers;

[ApiController]
[Route("api")]
public class BookingController : ControllerBase
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        IBookingRepository bookingRepository,
        ILogger<BookingController> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    [Authorize(Roles = "Member")]
    [HttpPost] // Medlem tilmelder sig en holdtræning
    [Route("members/{memberId}/bookings/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBooking(
        int memberId,
        int sessionId)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(CreateBooking));

        var booking = await _bookingRepository
            .CreateBooking(memberId, sessionId);

        if (booking == null)
        {
            _logger.LogWarning(
                "Kunne ikke oprette booking for medlem {MemberId} på hold {SessionId}",
                memberId,
                sessionId);

            return BadRequest("Booking failed");
        }

        _logger.LogInformation(
            "Booking {BookingId} blev oprettet for medlem {MemberId}",
            booking.BookingId,
            memberId);

        return Ok(booking);
    }
    [Authorize(Roles = "Member")]
    [HttpPut]
    [Route("members/{memberId}/sessions/{sessionId}/cancel")]
    public async Task<IActionResult> CancelBookingBySessionId(
        int memberId,
        int sessionId)
    {
        var result = await _bookingRepository
            .CancelBookingBySessionId(memberId, sessionId);

        if (!result)
            return BadRequest("Cancel booking failed");

        return Ok();
    }
}