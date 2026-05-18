using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SessionService.Models;
using SessionService.Repository.Interfaces;

namespace SessionService.Repository;

public class CenterRepositoryMongoDb : ICenterRepository
{
    private readonly IMongoCollection<Room> _roomCollection;
    private readonly IMongoCollection<FitnessCenter> _fitnessCenterCollection;

    private readonly ILogger<CenterRepositoryMongoDb> _logger;

    public CenterRepositoryMongoDb(
        IMongoDatabase database,
        ILogger<CenterRepositoryMongoDb> logger)
    {
        _logger = logger;

        _roomCollection = database.GetCollection<Room>("Rooms"); //Henter rooms collection fra databasen
        _fitnessCenterCollection = database.GetCollection<FitnessCenter>("FitnessCenters"); //Henter fitnesscenter collection fra databasen
    }
//Opret room
    public async Task<bool> CreateRoom(Room room)
    {
        try
        {
            var highestRoom = await _roomCollection
                .Find(Builders<Room>.Filter.Empty) //Finder alle lokaler
                .SortByDescending(x => x.RoomId) //Sorterer efter højeste id
                .Limit(1) //Tager kun den højeste
                .FirstOrDefaultAsync(); //Henter første resultat

            room.RoomId = (highestRoom?.RoomId ?? 0) + 1; //Sætter nyt lokale id til max +1

            await _roomCollection.InsertOneAsync(room); //Indsætter lokale i databasen

            _logger.LogInformation(
                "Lokale {RoomId} blev oprettet",
                room.RoomId); //Logger hvilket lokale der blev oprettet

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved oprettelse af lokale"); //Logger fejl hvis noget fejler
            return false;
        }
    }

//Hent alle rooms
    public async Task<List<Room>> GetAllRooms()
    {
        try
        {
            return await _roomCollection
                .Find(Builders<Room>.Filter.Empty) //Finder alle lokaler
                .ToListAsync(); //Laver resultat om til liste
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved hentning af lokaler"); //Logger fejl ved hentning
            return new List<Room>(); //Returnerer tom liste ved fejl
        }
    }

//Hent et room udfra id
    public async Task<Room?> GetRoomById(int roomId)
    {
        try
        {
            var filter = Builders<Room>
                .Filter.Eq(x => x.RoomId, roomId); //Finder lokale med matching id

            return await _roomCollection
                .Find(filter) //Bruger filter til at finde lokale
                .FirstOrDefaultAsync(); //Returnerer første match eller null
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved hentning af lokale {RoomId}", roomId); //Logger hvilket lokale der fejlede
            return null; //Returnerer null ved fejl
        }
    }

//Opret center
    public async Task<bool> CreateFitnessCenter(FitnessCenter center)
    {
        try
        {
            var highestCenter = await _fitnessCenterCollection
                .Find(Builders<FitnessCenter>.Filter.Empty) //Finder alle fitnesscentre
                .SortByDescending(x => x.FitnessCenterId) //Sorterer efter højeste id
                .Limit(1) //Tager kun den højeste
                .FirstOrDefaultAsync(); //Henter første resultat

            center.FitnessCenterId = (highestCenter?.FitnessCenterId ?? 0) + 1; //Sætter nyt fitnesscenter id til max +1

            await _fitnessCenterCollection.InsertOneAsync(center); //Indsætter fitnesscenter i databasen

            _logger.LogInformation(
                "Fitnesscenter {FitnessCenterId} blev oprettet",
                center.FitnessCenterId); //Logger hvilket fitnesscenter der blev oprettet

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved oprettelse af fitnesscenter"); //Logger fejl ved oprettelse
            return false;
        }
    }

//Hent alle centre
    public async Task<List<FitnessCenter>> GetAllFitnessCenters()
    {
        try
        {
            return await _fitnessCenterCollection
                .Find(Builders<FitnessCenter>.Filter.Empty) //Finder alle fitnesscentre
                .ToListAsync(); //Laver resultat om til liste
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fejl ved hentning af fitnesscentre"); //Logger fejl ved hentning
            return new List<FitnessCenter>(); //Returnerer tom liste ved fejl
        }
    }

//Hent et center udfra id
    public async Task<FitnessCenter?> GetFitnessCenterById(int fitnessCenterId)
    {
        try
        {
            var filter = Builders<FitnessCenter>
                .Filter.Eq(x => x.FitnessCenterId, fitnessCenterId); //Finder fitnesscenter med matching id

            return await _fitnessCenterCollection
                .Find(filter) //Bruger filter til at finde fitnesscenter
                .FirstOrDefaultAsync(); //Returnerer første match eller null
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Fejl ved hentning af fitnesscenter {FitnessCenterId}",
                fitnessCenterId); //Logger hvilket fitnesscenter der fejlede

            return null; //Returnerer null ved fejl
        }
    }
}