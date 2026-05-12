using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "Member,Admin,Trainer")]
    [HttpGet] // Hent alle sessions
    [Route("session")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllSessions()
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(GetAllSessions));

        var sessions = await _sessionRepository.GetAllSessions();

        _logger.LogInformation(
            "Hentede {SessionCount} sessions",
            sessions.Count);

        return Ok(sessions);
    }

    [Authorize(Roles = "Member,Admin,Trainer")]
    [HttpGet] // Hent en session udfra id
    [Route("session/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSessionById(int sessionId)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(GetSessionById));

        var session = await _sessionRepository
            .GetSessionById(sessionId);

        if (session == null)
        {
            _logger.LogWarning(
                "Session {SessionId} blev ikke fundet",
                sessionId);

            return NotFound("Session not found");
        }

        _logger.LogInformation(
            "Hentede session {SessionId}",
            sessionId);

        return Ok(session);
    }

    [Authorize(Roles = "Member")]
    [HttpGet] // Hent alle sessions som en member har tilmeldt sig
    [Route("members/{memberId}/sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSessionsByMemberId(int memberId)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(GetSessionsByMemberId));

        var sessions = await _sessionRepository
            .GetSessionsByMemberId(memberId);

        _logger.LogInformation(
            "Hentede {SessionCount} sessions for medlem {MemberId}",
            sessions.Count,
            memberId);

        return Ok(sessions);
    }

    [Authorize(Roles = "Trainer,Admin")]
    [HttpPost] // Opret session
    [Route("session")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSession(Session session)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(CreateSession));

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

    [Authorize(Roles = "Trainer,Admin")]
    [HttpPut] // Opdater session
    [Route("session/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateSession(
        int sessionId,
        Session updatedSession)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(UpdateSession));

        var result = await _sessionRepository
            .UpdateSession(sessionId, updatedSession);

        if (!result)
        {
            _logger.LogWarning(
                "Kunne ikke opdatere session {SessionId}",
                sessionId);

            return BadRequest("Failed to update session");
        }

        _logger.LogInformation(
            "Session {SessionId} blev opdateret",
            sessionId);

        return Ok();
    }

    [Authorize(Roles = "Trainer,Admin")]
    [HttpDelete] // Delete/fjern/slet session
    [Route("session/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteSession(int sessionId)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(DeleteSession));

        var result = await _sessionRepository
            .DeleteSession(sessionId);

        if (!result)
        {
            _logger.LogWarning(
                "Kunne ikke slette session {SessionId}",
                sessionId);

            return BadRequest("Failed to delete session");
        }

        _logger.LogInformation(
            "Session {SessionId} blev slettet",
            sessionId);

        return Ok();
    }
}