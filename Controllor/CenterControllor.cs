using Microsoft.AspNetCore.Mvc;
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

    [HttpPost] // Opret et room
    [Route("rooms")]
    public async Task<IActionResult> CreateRoom(Room room)
    {
        if (string.IsNullOrWhiteSpace(room.RoomName))
        {
            _logger.LogWarning("Lokale kunne ikke oprettes fordi navn mangler");
            return BadRequest("Room name is required");
        }

        var result = await _centerRepository.CreateRoom(room);

        if (!result)
        {
            _logger.LogWarning("Kunne ikke oprette lokale {RoomName}", room.RoomName);
            return BadRequest("Failed to create room");
        }

        _logger.LogInformation("Lokale {RoomName} blev oprettet", room.RoomName);
        return Created();
    }

    [HttpGet] // Hent alle rooms
    [Route("rooms")]
    public async Task<IActionResult> GetAllRooms()
    {
        var rooms = await _centerRepository.GetAllRooms();

        _logger.LogInformation("Hentede {RoomCount} lokaler", rooms.Count);
        return Ok(rooms);
    }

    [HttpGet] // Hent et room udfra id
    [Route("rooms/{roomId}")]
    public async Task<IActionResult> GetRoomById(int roomId)
    {
        var room = await _centerRepository.GetRoomById(roomId);

        if (room == null)
        {
            _logger.LogWarning("Lokale {RoomId} blev ikke fundet", roomId);
            return NotFound("Room not found");
        }

        _logger.LogInformation("Hentede lokale {RoomId}", roomId);
        return Ok(room);
    }

    [HttpPost] // Opret et center
    [Route("fitnesscenters")]
    public async Task<IActionResult> CreateFitnessCenter(FitnessCenter center)
    {
        if (string.IsNullOrWhiteSpace(center.adresse))
        {
            _logger.LogWarning("Fitnesscenter kunne ikke oprettes fordi adresse mangler");
            return BadRequest("Address is required");
        }

        var result = await _centerRepository.CreateFitnessCenter(center);

        if (!result)
        {
            _logger.LogWarning("Kunne ikke oprette fitnesscenter på {Address}", center.adresse);
            return BadRequest("Failed to create fitness center");
        }

        _logger.LogInformation("Fitnesscenter på {Address} blev oprettet", center.adresse);
        return Created();
    }

    [HttpGet] // Hent alle fitness centers
    [Route("fitnesscenters")]
    public async Task<IActionResult> GetAllFitnessCenters()
    {
        var centers = await _centerRepository.GetAllFitnessCenters();

        _logger.LogInformation("Hentede {CenterCount} fitnesscentre", centers.Count);
        return Ok(centers);
    }

    [HttpGet] // Hent et fitness center udfra id
    [Route("fitnesscenters/{fitnessCenterId}")]
    public async Task<IActionResult> GetFitnessCenterById(int fitnessCenterId)
    {
        var center = await _centerRepository.GetFitnessCenterById(fitnessCenterId);

        if (center == null)
        {
            _logger.LogWarning("Fitnesscenter {FitnessCenterId} blev ikke fundet", fitnessCenterId);
            return NotFound("Fitness center not found");
        }

        _logger.LogInformation("Hentede fitnesscenter {FitnessCenterId}", fitnessCenterId);
        return Ok(center);
    }
}