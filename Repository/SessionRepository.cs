using MongoDB.Driver;
using SessionService.Models;
using SessionService.Repository.Interfaces;

namespace SessionService.Repository;

public class SessionRepositoryMongoDb : ISessionRepository
{
    private readonly IMongoCollection<Session> _sessionCollection;

    private readonly IMongoCollection<Booking> _bookingCollection;

    private readonly ILogger<SessionRepositoryMongoDb> _logger;

    private readonly IHttpClientFactory _httpClientFactory;

    public SessionRepositoryMongoDb(
        IMongoDatabase database,
        ILogger<SessionRepositoryMongoDb> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        _sessionCollection = database.GetCollection<Session>("Sessions");
        _bookingCollection = database.GetCollection<Booking>("Bookings");
    }

    //Hent alle holdtræninger
    public async Task<List<Session>> GetAllSessions()
    {
        try
        {
            return await _sessionCollection
                .Find(Builders<Session>.Filter.Empty) //Finder alle sessions
                .ToListAsync(); //Laver om til liste
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved hentning af sessions"); //Logger hvis noget fejler
            return new List<Session>(); //Returnerer tom liste hvis fejl
        }
    }

    //Find enkelt holdtræning udfra id
    public async Task<Session?> GetSessionById(int sessionId)
    {
        try
        {
            var filter = Builders<Session>
                .Filter.Eq(x => x.SessionId, sessionId); //Finder session med matching id

            return await _sessionCollection
                .Find(filter) //Bruger filteret til at finde session
                .FirstOrDefaultAsync(); //Returnerer første match eller null
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved hentning af session {SessionId}", sessionId); //Logger fejl med session id
            return null; //Returnerer null hvis fejl
        }
    }

    //Find alle sessions som en member har tilmeldt sig
    public async Task<List<Session>> GetSessionsByMemberId(int memberId)
    {
        try
        {
            _logger.LogDebug(
                "GetSessionsByMemberId called med memberId {MemberId}",
                memberId);

            // Find alle bookings for denne medlem
            var bookingFilter = Builders<Booking>
                .Filter.Eq(x => x.MemberId, memberId);

            var memberBookings = await _bookingCollection
                .Find(bookingFilter)
                .ToListAsync();

            if (!memberBookings.Any())
            {
                _logger.LogDebug("Ingen bookings fundet for medlem {MemberId}", memberId);
                return new List<Session>();
            }

            // Hent session IDs fra bookings
            var sessionIds = memberBookings.Select(b => b.SessionId).ToList();

            // Find alle sessions med disse IDs
            var sessionFilter = Builders<Session>
                .Filter.In(x => x.SessionId, sessionIds);

            var sessions = await _sessionCollection
                .Find(sessionFilter)
                .ToListAsync();

            _logger.LogDebug(
                "Fundet {SessionCount} sessions for medlem {MemberId}",
                sessions.Count,
                memberId);

            return sessions;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Fejl ved hentning af sessions for medlem {MemberId}",
                memberId);

            return new List<Session>();
        }
    }

    //Opret holdtræning
    public async Task<bool> CreateSession(Session session)
    {
        try
        {
            var userClient = _httpClientFactory.CreateClient("userService");

            //finder instruktør
            var instructorResponse = await userClient //Bruger userservice til at finde instruktor
                .GetAsync($"api/member/{session.InstructorId}"); //Selve request til usersericen

            if (!instructorResponse.IsSuccessStatusCode) //Hvis instruktør ikke findes
            {
                _logger.LogWarning(
                    "Instruktør {InstructorId} blev ikke fundet",
                    session.InstructorId);

                return false;
            }

            var highestSession = await _sessionCollection
                .Find(Builders<Session>.Filter.Empty)
                .SortByDescending(x => x.SessionId)
                .Limit(1)
                .FirstOrDefaultAsync(); //Finder max id

            session.SessionId = (highestSession?.SessionId ?? 0) + 1; //Sætter den nye sessions id til at være maks +1

            // Selve oprettelsen
            await _sessionCollection.InsertOneAsync(session);

            _logger.LogInformation(
                "Session {SessionId} blev oprettet af {instructorId}", //Nævner hvem der har oprettet i logger
                session.SessionId);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved oprettelse af session"); //Hvis smth andet fejler
            return false;
        }
    }

    //Redigere holdtræning
    public async Task<bool> UpdateSession(
        int sessionId,
        Session updatedSession)
    {
        try
        {
            var filter = Builders<Session>
                .Filter.Eq(x => x.SessionId, sessionId); //Finder session som skal opdateres

            updatedSession.SessionId = sessionId; //Sikrer at id ikke ændres

            var result = await _sessionCollection
                .ReplaceOneAsync(filter, updatedSession); //Erstatter gammel session med ny data

            if (result.ModifiedCount > 0) //Hvis noget faktisk blev ændret
            {
                _logger.LogInformation(
                    "Session {SessionId} blev opdateret",
                    sessionId); //Logger hvilken session der blev opdateret

                return true;
            }

            return false; //Returnerer false hvis intet blev ændret
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved opdatering af session {SessionId}", sessionId); //Logger fejl ved update
            return false;
        }
    }

    //Slet holdtræning
    public async Task<bool> DeleteSession(int sessionId)
    {
        try
        {
            var filter = Builders<Session>
                .Filter.Eq(x => x.SessionId, sessionId); //Finder session som skal slettes

            var result = await _sessionCollection
                .DeleteOneAsync(filter); //Sletter session fra databasen

            if (result.DeletedCount > 0) //Hvis session blev slettet
            {
                _logger.LogInformation(
                    "Session {SessionId} blev slettet",
                    sessionId); //Logger hvilken session der blev slettet

                return true;
            }

            return false; //Returnerer false hvis ingen session blev slettet
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved sletning af session {SessionId}", sessionId); //Logger fejl ved sletning
            return false;
        }
    }
}