using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SessionService.Models;

public class Room
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int RoomId { get; set; }

    public string RoomName { get; set; }

    public int Capacity { get; set; }
    
    public int fitnessCenterId { get; set; }
}

//oprettes her