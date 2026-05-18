using System.Collections.Generic;
using System.Threading.Tasks;
using SessionService.Models;

namespace SessionService.Repository.Interfaces;

public interface ICenterRepository
{
    // Roomstask
    Task<bool> CreateRoom(Room room);

    Task<List<Room>> GetAllRooms();

    Task<Room?> GetRoomById(int roomId);

    
    // Centertask
    Task<bool> CreateFitnessCenter(FitnessCenter center);

    Task<List<FitnessCenter>> GetAllFitnessCenters();

    Task<FitnessCenter?> GetFitnessCenterById(int fitnessCenterId);
}
//1 OPRET ROOM
//Opret et room
//2 CENTER
//Opret et center
//3 HENT ALLE VÆRELSER
//(til når træner skal oprette en session så kan han se hvilke rooms der er tilgængelige i en dato) Frontend finder selv ud af dato)
//GET /api/rooms
//4 HENT ET VÆRELSE
// Hente et specifikt room
// GET /api/rooms/{roomId}
//5 HENT ALLE CENTRE
// GET /api/fitnesscenters
//6 HENT ET CENTER
//GET /api/fitnesscenters/{fitnessCenterId}