using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SessionService.Models;

public class FitnessCenter
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int FitnessCenterId  { get; set; }
    public string adresse { get; set; }
}
//siger sig selv
