using Microsoft.AspNetCore.Mvc;
using SessionService.Models;
using SessionService.Repository.Interfaces;

namespace SessionService.Controllers;

[ApiController]
[Route("api")]
public class SessionController : ControllerBase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<SessionController> _logger;

    public SessionController(
        ISessionRepository sessionRepository,
        ILogger<SessionController> logger)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    [HttpGet] // Hent alle sessions
    [Route("session")]
    public async Task<IActionResult> GetAllSessions()
    {
        var sessions = await _sessionRepository.GetAllSessions();

        _logger.LogInformation("Hentede {SessionCount} sessions", sessions.Count);
        return Ok(sessions);
    }

    [HttpGet] // Hent en session udfra id
    [Route("session/{sessionId}")]
    public async Task<IActionResult> GetSessionById(int sessionId)
    {
        var session = await _sessionRepository
            .GetSessionById(sessionId);

        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} blev ikke fundet", sessionId);
            return NotFound("Session not found");
        }

        _logger.LogInformation("Hentede session {SessionId}", sessionId);
        return Ok(session);
    }

    [HttpGet] // Hent alle sessions som en member har tilmeldt sig
    [Route("members/{memberId}/sessions")]
    public async Task<IActionResult> GetSessionsByMemberId(int memberId)
    {
        var sessions = await _sessionRepository
            .GetSessionsByMemberId(memberId);

        _logger.LogInformation(
            "Hentede {SessionCount} sessions for medlem {MemberId}",
            sessions.Count, memberId);

        return Ok(sessions);
    }

    [HttpPost] // Opret session
    [Route("session")]
    public async Task<IActionResult> CreateSession(Session session)
    {
        var result = await _sessionRepository
            .CreateSession(session);

        if (!result)
        {
            _logger.LogWarning(
                "Kunne ikke oprette session for instruktør {InstructorId}",
                session.InstructorId);

            return BadRequest("Failed to create session");
        }

        _logger.LogInformation(
            "Session blev oprettet for instruktør {InstructorId}",
            session.InstructorId);

        return Created();
    }

    [HttpPut] // Opdater session
    [Route("session/{sessionId}")]
    public async Task<IActionResult> UpdateSession(
        int sessionId,
        Session updatedSession)
    {
        var result = await _sessionRepository
            .UpdateSession(sessionId, updatedSession);

        if (!result)
        {
            _logger.LogWarning("Kunne ikke opdatere session {SessionId}", sessionId);
            return BadRequest("Failed to update session");
        }

        _logger.LogInformation("Session {SessionId} blev opdateret", sessionId);
        return Ok();
    }

    [HttpDelete] // Delete/fjern/slet session
    [Route("session/{sessionId}")]
    public async Task<IActionResult> DeleteSession(int sessionId)
    {
        var result = await _sessionRepository
            .DeleteSession(sessionId);

        if (!result)
        {
            _logger.LogWarning("Kunne ikke slette session {SessionId}", sessionId);
            return BadRequest("Failed to delete session");
        }

        _logger.LogInformation("Session {SessionId} blev slettet", sessionId);
        return Ok();
    }
}