namespace SessionService.Models;

public class Session
{
    public int SessionId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int InstructorId { get; set; } //Der kan kun være en instruktør på en hold træning

    public int RoomId { get; set; }

    public int Capacity { get; set; } 

    public TeamSessionStatus Status { get; set; }
    
    
}
public enum TeamSessionStatus
{
    Cancelled,
    Available,
    Full
}

//modtager fra ekstern Medlem.cs som sender - public int MemberId { get; set; } public int SessionId { get; set; } som request
//Med de to oprettes booking.cs
//Session.cs modtager booking.cs
//finder ud af om der er plads, hvis der ikke er plads slettes booking.cs
//Hvis der er plads fastholdes den og der tilføjes i capacity

//Træner opretter session
//Træner hentes fra user



// Man kan søge for antal af tilmeldte i en session ved at søge
// Hvor mange bookinger har x som sessionId


