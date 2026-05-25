using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SessionService.Models;

public class Booking
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int BookingId { get; set; } //oprettes

    public int MemberId { get; set; } //Modtages

    public int SessionId { get; set; } //Modtages

}