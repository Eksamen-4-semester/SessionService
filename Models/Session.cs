using System;

namespace SessionService.Models;

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class Session
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("SessionId")]
    public int SessionId { get; set; }

    [BsonElement("SessionName")]
    public string SessionName { get; set; }

    [BsonElement("StartTime")]
    public DateTime StartTime { get; set; }

    [BsonElement("EndTime")]
    public DateTime EndTime { get; set; }

    [BsonElement("InstructorId")]
    public int InstructorId { get; set; }

    [BsonElement("RoomId")]
    public int RoomId { get; set; }

    [BsonElement("CurrentCapacity")]
    public int CurrentCapacity { get; set; }

    [BsonElement("MaxCapacity")]
    public int MaxCapacity { get; set; }

    [BsonElement("Status")]
    public TeamSessionStatus Status { get; set; }
}

public enum TeamSessionStatus
{
    Cancelled,
    Available,
    Full
}


//tip
// Man kan søge for antal af tilmeldte i en session ved at søge
// Hvor mange bookinger har x som sessionId