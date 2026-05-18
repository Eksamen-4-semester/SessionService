using System.Collections.Generic;
using System.Threading.Tasks;
using SessionService.Models;

namespace SessionService.Repository.Interfaces;

public interface ISessionRepository
{
    Task<List<Session>> GetAllSessions();
    Task<Session?> GetSessionById(int sessionId);
    Task<List<Session>> GetSessionsByMemberId(int memberId);
    Task<bool> CreateSession(Session session);
    Task<bool> UpdateSession(int sessionId, Session updatedSession);
    Task<bool> DeleteSession(int sessionId);
}

//1 Hent alle sessions
//Hent alle bookinger: GET /api/session
//Return: Statuskode + Liste af sessioner
//2 Hent en session
//GET /api/session/{sessionId}
//Return: Statuskode + Booking
//3 Hent alle sessions som en specifik medlem er tilmeldt
// GET
///api/members/{memberId}/sessions/ 
//Return: Statuskode + Liste af sessions hvor vedkommende har en booking
//4 opret en session
// POST /api/session
//Return: Statuskode + session
//Træner opretter session
//Træner hentes fra user service, og der tjekkes om det er en træner, hvis det er en træner oprettes sessionen, hvis ikke returneres en error

//5 Redigere session
// kan kun gøres som træner
//PUT /api/session/{sessionId}
//6 delete session
// Kan kun gøres som træner
//DELETE /api/session/{sessionId}