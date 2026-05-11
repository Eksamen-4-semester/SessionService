using Microsoft.AspNetCore.Mvc;
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

    [HttpPost] // Medlem tilmelder sig en holdtræning
    [Route("members/{memberId}/bookings/{sessionId}")]
    public async Task<IActionResult> CreateBooking(
        int memberId,
        int sessionId)
    {
        var booking = await _bookingRepository
            .CreateBooking(memberId, sessionId);

        if (booking == null)
        {
            _logger.LogWarning(
                "Kunne ikke oprette booking for medlem {MemberId} på hold {SessionId}",
                memberId, sessionId);

            return BadRequest("Booking failed");
        }

        _logger.LogInformation(
            "Booking {BookingId} blev oprettet for medlem {MemberId}",
            booking.BookingId, memberId);

        return Ok(booking);
    }

    [HttpPut] // Medlem afmelder sig en holdtræning
    [Route("members/{memberId}/bookings/{bookingId}/cancel")]
    public async Task<IActionResult> CancelBooking(
        int memberId,
        int bookingId)
    {
        var result = await _bookingRepository
            .CancelBooking(memberId, bookingId);

        if (!result)
        {
            _logger.LogWarning(
                "Kunne ikke annullere booking {BookingId} for medlem {MemberId}",
                bookingId, memberId);

            return BadRequest("Cancel booking failed");
        }

        _logger.LogInformation(
            "Booking {BookingId} blev annulleret af medlem {MemberId}",
            bookingId, memberId);

        return Ok();
    }
}