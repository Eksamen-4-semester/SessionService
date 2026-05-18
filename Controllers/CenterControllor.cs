using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SessionService.Models;
using SessionService.Repository.Interfaces;

namespace SessionService.Controllers;

[ApiController]
[Route("api")]
public class CenterController : ControllerBase
{
    private readonly ICenterRepository _centerRepository;
    private readonly ILogger<CenterController> _logger;

    public CenterController(
        ICenterRepository centerRepository,
        ILogger<CenterController> logger)
    {
        _centerRepository = centerRepository;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost] // Opret et room
    [Route("rooms")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateRoom(Room room)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(CreateRoom));

        if (string.IsNullOrWhiteSpace(room.RoomName))
        {
            _logger.LogWarning(
                "Lokale kunne ikke oprettes fordi navn mangler");

            return BadRequest("Room name is required");
        }

        var result = await _centerRepository.CreateRoom(room);

        if (!result)
        {
            _logger.LogWarning(
                "Kunne ikke oprette lokale {RoomName}",
                room.RoomName);

            return BadRequest("Failed to create room");
        }

        _logger.LogInformation(
            "Lokale {RoomName} blev oprettet",
            room.RoomName);

        return Created();
    }

    [Authorize(Roles = "Member,Admin,Trainer")]
    [HttpGet] // Hent alle rooms
    [Route("rooms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllRooms()
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(GetAllRooms));

        var rooms = await _centerRepository.GetAllRooms();

        _logger.LogInformation(
            "Hentede {RoomCount} lokaler",
            rooms.Count);

        return Ok(rooms);
    }

    [Authorize(Roles = "Member,Admin,Trainer")]
    [HttpGet] // Hent et room udfra id
    [Route("rooms/{roomId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRoomById(int roomId)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(GetRoomById));

        var room = await _centerRepository.GetRoomById(roomId);

        if (room == null)
        {
            _logger.LogWarning(
                "Lokale {RoomId} blev ikke fundet",
                roomId);

            return NotFound("Room not found");
        }

        _logger.LogInformation(
            "Hentede lokale {RoomId}",
            roomId);

        return Ok(room);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost] // Opret et center
    [Route("fitnesscenters")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFitnessCenter(FitnessCenter center)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(CreateFitnessCenter));

        if (string.IsNullOrWhiteSpace(center.adresse))
        {
            _logger.LogWarning(
                "Fitnesscenter kunne ikke oprettes fordi adresse mangler");

            return BadRequest("Address is required");
        }

        var result = await _centerRepository.CreateFitnessCenter(center);

        if (!result)
        {
            _logger.LogWarning(
                "Kunne ikke oprette fitnesscenter på {Address}",
                center.adresse);

            return BadRequest("Failed to create fitness center");
        }

        _logger.LogInformation(
            "Fitnesscenter på {Address} blev oprettet",
            center.adresse);

        return Created();
    }

    [Authorize(Roles = "Member,Admin,Trainer")]
    [HttpGet] // Hent alle fitness centers
    [Route("fitnesscenters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllFitnessCenters()
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(GetAllFitnessCenters));

        var centers = await _centerRepository.GetAllFitnessCenters();

        _logger.LogInformation(
            "Hentede {CenterCount} fitnesscentre",
            centers.Count);

        return Ok(centers);
    }

    [Authorize(Roles = "Member,Admin,Trainer")]
    [HttpGet] // Hent et fitness center udfra id
    [Route("fitnesscenters/{fitnessCenterId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFitnessCenterById(int fitnessCenterId)
    {
        _logger.LogInformation(
            "Called {function} endpoint",
            nameof(GetFitnessCenterById));

        var center = await _centerRepository.GetFitnessCenterById(fitnessCenterId);

        if (center == null)
        {
            _logger.LogWarning(
                "Fitnesscenter {FitnessCenterId} blev ikke fundet",
                fitnessCenterId);

            return NotFound("Fitness center not found");
        }

        _logger.LogInformation(
            "Hentede fitnesscenter {FitnessCenterId}",
            fitnessCenterId);

        return Ok(center);
    }
}