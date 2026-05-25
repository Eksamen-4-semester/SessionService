using System.Threading.Tasks;
using SessionService.Models;

namespace SessionService.Repository.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> CreateBooking(
        int memberId,
        int sessionId);
    Task<bool> CancelBooking( //Det er bare en delete
        int memberId,
        int bookingId);
    Task<bool> CancelBookingBySessionId(int memberId, int sessionId);
}
//1 TILMELDING
//modtager fra ekstern Medlem.cs som sender - public int MemberId { get; set; } public int SessionId { get; set; } som request
//Med de to oprettes booking.cs
//Session.cs modtager booking.cs
//finder ud af om der er plads, hvis der ikke er plads slettes booking.cs
//Hvis der er plads fastholdes den og der tilføjes i capacity
//En person tilmelder sig en session:
//POST /api/members/{memberId}/bookings/{bookingId}
//Return: Statuskode + Booking
//2 AFMELDING
//En person sletter/afmelder sig en session: DELETE /api/members/{memberId}/bookings/{bookingId}
//Afmeld en booking: PUT /api/members/{memberId}/bookings/{bookingId}/cancel Return: Statuskode + Booking (Det er en PUT så man kan se hvor mange der afmelder sig, kunne man ikke hvis det bare var en delete)
