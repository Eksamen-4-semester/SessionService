using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using SessionService.Models;

namespace SessionService.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IMongoDatabase database)
    {
        var sessionCollection = database.GetCollection<Session>("Sessions");
        var roomCollection = database.GetCollection<Room>("Rooms");
        var centerCollection = database.GetCollection<FitnessCenter>("FitnessCenters");
        var bookingCollection = database.GetCollection<Booking>("Bookings");

        // FITNESS CENTERS
        if (!await centerCollection.Find(_ => true).AnyAsync())
        {
            await centerCollection.InsertManyAsync(new List<FitnessCenter>
            {
                new() { FitnessCenterId = 1, adresse = "FitLife Aarhus C" },
                new() { FitnessCenterId = 2, adresse = "FitLife Viby" }
            });
        }

        // ROOMS
        if (!await roomCollection.Find(_ => true).AnyAsync())
        {
            await roomCollection.InsertManyAsync(new List<Room>
            {
                new() { RoomId = 1, RoomName = "Yoga Sal", Capacity = 20, fitnessCenterId = 1 },
                new() { RoomId = 2, RoomName = "HIIT Arena", Capacity = 15, fitnessCenterId = 1 },
                new() { RoomId = 3, RoomName = "Spinning", Capacity = 25, fitnessCenterId = 2 },
                new() { RoomId = 4, RoomName = "Functional Zone", Capacity = 18, fitnessCenterId = 2 }
            });
        }

        // SESSIONS
        var sessions = await sessionCollection.Find(_ => true).ToListAsync();

        var shouldRefreshSessions =
            !sessions.Any() ||
            sessions.All(x => x.StartTime.Date < DateTime.Now.Date.AddDays(-7));

        if (shouldRefreshSessions)
        {
            await sessionCollection.DeleteManyAsync(_ => true);
            await bookingCollection.DeleteManyAsync(_ => true);

            var monday = GetCurrentMonday();

            var newSessions = new List<Session>
            {
                new()
                {
                    SessionId = 1,
                    SessionName = "Morning Yoga",
                    StartTime = monday.AddDays(1).AddHours(7),
                    EndTime = monday.AddDays(1).AddHours(8),
                    InstructorId = 1,
                    RoomId = 1,
                    CurrentCapacity = 12,
                    MaxCapacity = 20,
                    Status = TeamSessionStatus.Available
                },
                new()
                {
                    SessionId = 2,
                    SessionName = "HIIT Burn",
                    StartTime = monday.AddDays(1).AddHours(18),
                    EndTime = monday.AddDays(1).AddHours(19),
                    InstructorId = 2,
                    RoomId = 2,
                    CurrentCapacity = 15,
                    MaxCapacity = 15,
                    Status = TeamSessionStatus.Full
                },
                new()
                {
                    SessionId = 3,
                    SessionName = "Power Spin",
                    StartTime = monday.AddDays(2).AddHours(17),
                    EndTime = monday.AddDays(2).AddHours(18),
                    InstructorId = 1,
                    RoomId = 3,
                    CurrentCapacity = 10,
                    MaxCapacity = 25,
                    Status = TeamSessionStatus.Available
                },
                new()
                {
                    SessionId = 4,
                    SessionName = "Functional Strength",
                    StartTime = monday.AddDays(3).AddHours(16),
                    EndTime = monday.AddDays(3).AddHours(17),
                    InstructorId = 2,
                    RoomId = 4,
                    CurrentCapacity = 5,
                    MaxCapacity = 18,
                    Status = TeamSessionStatus.Available
                }
            };

            await sessionCollection.InsertManyAsync(newSessions);

            await bookingCollection.InsertManyAsync(new List<Booking>
            {
                new() { BookingId = 1, MemberId = 1, SessionId = 1 },
                new() { BookingId = 2, MemberId = 1, SessionId = 3 },
                new() { BookingId = 3, MemberId = 2, SessionId = 2 }
            });
        }
    }

    private static DateTime GetCurrentMonday()
    {
        var today = DateTime.Now.Date;
        var diff = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-diff);
    }
}