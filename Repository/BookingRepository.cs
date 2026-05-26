using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SessionService.Models;
using SessionService.Repository.Interfaces;

namespace SessionService.Repository;

public class BookingRepositoryMongoDb : IBookingRepository
{
    private readonly IMongoCollection<Booking> _bookingCollection;
    private readonly IMongoCollection<Session> _sessionCollection;
    private readonly IMongoCollection<Room> _roomCollection;

    private readonly ILogger<BookingRepositoryMongoDb> _logger;

    public BookingRepositoryMongoDb(
        IMongoDatabase database,
        ILogger<BookingRepositoryMongoDb> logger)
    {
        _logger = logger;

        _bookingCollection = database.GetCollection<Booking>("Bookings");
        _sessionCollection = database.GetCollection<Session>("Sessions");
        _roomCollection = database.GetCollection<Room>("Rooms");
    }

    public async Task<Booking?> CreateBooking(int memberId, int sessionId)
    {
        try
        {
            var sessionFilter = Builders<Session>
                .Filter.Eq(x => x.SessionId, sessionId);

            var session = await _sessionCollection
                .Find(sessionFilter)
                .FirstOrDefaultAsync();

            if (session == null)
                return null;

            if (session.CurrentCapacity >= session.MaxCapacity)
                return null;

            var alreadyBookedFilter = Builders<Booking>.Filter.And(
                Builders<Booking>.Filter.Eq(x => x.MemberId, memberId),
                Builders<Booking>.Filter.Eq(x => x.SessionId, sessionId)
            );

            var alreadyBooked = await _bookingCollection
                .Find(alreadyBookedFilter)
                .AnyAsync();

            if (alreadyBooked)
                return null;

            var booking = new Booking
            {
                BookingId = (
                    await _bookingCollection
                        .Find(Builders<Booking>.Filter.Empty)
                        .SortByDescending(x => x.BookingId)
                        .Limit(1)
                        .FirstOrDefaultAsync()
                )?.BookingId + 1 ?? 1,

                MemberId = memberId,
                SessionId = sessionId
            };

            await _bookingCollection.InsertOneAsync(booking);

            session.CurrentCapacity++;

            session.Status = session.CurrentCapacity >= session.MaxCapacity
                ? TeamSessionStatus.Full
                : TeamSessionStatus.Available;

            await _sessionCollection.ReplaceOneAsync(sessionFilter, session);

            _logger.LogInformation(
                "Booking {BookingId} blev oprettet",
                booking.BookingId);

            return booking;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved oprettelse af booking");
            return null;
        }
    }

    public async Task<bool> CancelBookingBySessionId(int memberId, int sessionId)
    {
        var bookingFilter = Builders<Booking>.Filter.And(
            Builders<Booking>.Filter.Eq(x => x.MemberId, memberId),
            Builders<Booking>.Filter.Eq(x => x.SessionId, sessionId)
        );

        var result = await _bookingCollection.DeleteOneAsync(bookingFilter);

        if (result.DeletedCount == 0)
            return false;

        var sessionFilter = Builders<Session>
            .Filter.Eq(x => x.SessionId, sessionId);

        var session = await _sessionCollection
            .Find(sessionFilter)
            .FirstOrDefaultAsync();

        if (session == null)
            return true;

        if (session.CurrentCapacity > 0)
            session.CurrentCapacity--;

        session.Status = session.CurrentCapacity >= session.MaxCapacity
            ? TeamSessionStatus.Full
            : TeamSessionStatus.Available;

        await _sessionCollection.ReplaceOneAsync(sessionFilter, session);

        return true;
    }
}