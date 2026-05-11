namespace SessionService.Models;

public class Session //En session = En hold træning
{
    public int SessionId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int InstructorId { get; set; } //Der kan kun være en instruktør på en hold træning

    public int RoomId { get; set; }

    public int CurrentCapacity { get; set; }
    
    public int MaxCapacity { get; set; }

    public TeamSessionStatus Status { get; set; }
    
    public List<int> MemberIds { get; set; } = new List<int>();
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