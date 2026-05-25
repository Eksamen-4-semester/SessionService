using System;
using System.Net.Http;
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
    private readonly IHttpClientFactory _httpClientFactory;

    public BookingRepositoryMongoDb(
        IMongoDatabase database,
        ILogger<BookingRepositoryMongoDb> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        _bookingCollection = database.GetCollection<Booking>("Bookings"); //Henter bookings collection fra databasen
        _sessionCollection = database.GetCollection<Session>("Sessions"); //Henter sessions collection fra databasen
        _roomCollection = database.GetCollection<Room>("Rooms"); //Henter rooms collection fra databasen
    }
//opret booking
    public async Task<Booking?> CreateBooking(
        int memberId,
        int sessionId)
    {
        try
        {
            // FIND SESSION
            var sessionFilter = Builders<Session>
                .Filter.Eq(x => x.SessionId, sessionId); //Finder session med matching id

            var session = await _sessionCollection
                .Find(sessionFilter) //Bruger filter til at finde session
                .FirstOrDefaultAsync(); //Returnerer første match eller null

            if (session == null) //Hvis session ikke findes
            {
                return null;
            }

            // FIND ROOM
            var roomFilter = Builders<Room>
                .Filter.Eq(x => x.RoomId, session.RoomId); //Finder lokale udfra sessionens room id

            var room = await _roomCollection
                .Find(roomFilter) //Bruger filter til at finde lokale
                .FirstOrDefaultAsync(); //Returnerer første match eller null

            if (room == null) //Hvis lokale ikke findes
            {
                return null;
            }

            // TJEK CAPACITY - tæl antal bookings for denne session
            var existingBookingsCount = await _bookingCollection
                .CountDocumentsAsync(Builders<Booking>.Filter.Eq(x => x.SessionId, sessionId)); //Tæller bookings for session

            if (existingBookingsCount >= room.Capacity) //Hvis lokalet er fyldt op
            {
                _logger.LogWarning(
                    "Session {SessionId} er fuldt booket",
                    sessionId); //Logger at session er fuld

                return null;
            }

            // OPRET BOOKING
            Booking booking = new Booking()
            {
                BookingId = (
                    await _bookingCollection
                        .Find(Builders<Booking>.Filter.Empty) //Finder alle bookings
                        .SortByDescending(x => x.BookingId) //Sorterer efter højeste id
                        .Limit(1) //Tager kun den højeste
                        .FirstOrDefaultAsync() //Henter første resultat
                )?.BookingId + 1 ?? 1, //Sætter booking id til max +1 eller 1 hvis ingen findes

                MemberId = memberId, //Gemmer member id
                SessionId = sessionId //Gemmer session id
            };

            await _bookingCollection.InsertOneAsync(booking); //Indsætter booking i databasen


            _logger.LogInformation(
                "Booking {BookingId} blev oprettet",
                booking.BookingId); //Logger hvilken booking der blev oprettet

            return booking;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved oprettelse af booking"); //Logger fejl ved booking
            return null;
        }
    }
    public async Task<bool> CancelBookingBySessionId(int memberId, int sessionId)
    {
        var filter = Builders<Booking>.Filter.And(
            Builders<Booking>.Filter.Eq(x => x.MemberId, memberId),
            Builders<Booking>.Filter.Eq(x => x.SessionId, sessionId)
        );

        var result = await _bookingCollection.DeleteOneAsync(filter);

        return result.DeletedCount > 0;
    }
//Afmeld booking
    public async Task<bool> CancelBooking(
        int memberId,
        int bookingId)
    {
        try
        {
            // FIND BOOKING
            var bookingFilter = Builders<Booking>
                .Filter.Eq(x => x.BookingId, bookingId); //Finder booking med matching id

            var booking = await _bookingCollection
                .Find(bookingFilter) //Bruger filter til at finde booking
                .FirstOrDefaultAsync(); //Returnerer første match eller null

            if (booking == null || booking.MemberId != memberId) //Hvis booking ikke findes eller ikke tilhører medlemmet
            {
                return false;
            }


            // DELETE BOOKING
            var deleteResult = await _bookingCollection
                .DeleteOneAsync(bookingFilter); //Sletter booking fra databasen

            if (deleteResult.DeletedCount > 0) //Hvis booking blev slettet
            {
                _logger.LogInformation(
                    "Booking {BookingId} blev annulleret",
                    bookingId); //Logger hvilken booking der blev annulleret

                return true;
            }

            return false; //Returnerer false hvis ingen booking blev slettet
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved annullering af booking"); //Logger fejl ved annullering
            return false;
        }
    }
}