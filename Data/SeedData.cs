using MongoDB.Driver;
using SessionService.Models;

namespace SessionService.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IMongoDatabase database)
    {
        var sessionCollection =
            database.GetCollection<Session>("Sessions");

        var roomCollection =
            database.GetCollection<Room>("Rooms");

        var centerCollection =
            database.GetCollection<FitnessCenter>("FitnessCenters");

        var bookingCollection =
            database.GetCollection<Booking>("Bookings");

        // FITNESS CENTERS
        var centersExist = await centerCollection
            .Find(_ => true)
            .AnyAsync();

        if (!centersExist)
        {
            var centers = new List<FitnessCenter>
            {
                new()
                {
                    FitnessCenterId = 1,
                    adresse = "FitLife Aarhus C"
                },
                new()
                {
                    FitnessCenterId = 2,
                    adresse = "FitLife Viby"
                }
            };

            await centerCollection.InsertManyAsync(centers);
        }

        // ROOMS
        var roomsExist = await roomCollection
            .Find(_ => true)
            .AnyAsync();

        if (!roomsExist)
        {
            var rooms = new List<Room>
            {
                new()
                {
                    RoomId = 1,
                    RoomName = "Yoga Sal",
                    Capacity = 20,
                    fitnessCenterId = 1
                },

                new()
                {
                    RoomId = 2,
                    RoomName = "HIIT Arena",
                    Capacity = 15,
                    fitnessCenterId = 1
                },

                new()
                {
                    RoomId = 3,
                    RoomName = "Spinning",
                    Capacity = 25,
                    fitnessCenterId = 2
                },

                new()
                {
                    RoomId = 4,
                    RoomName = "Functional Zone",
                    Capacity = 18,
                    fitnessCenterId = 2
                }
            };

            await roomCollection.InsertManyAsync(rooms);
        }

        // SESSIONS
        var sessionsExist = await sessionCollection
            .Find(_ => true)
            .AnyAsync();

        if (!sessionsExist)
        {
            var now = DateTime.Now;

            var sessions = new List<Session>
            {
                new()
                {
                    SessionId = 1,
                    SessionName = "Morning Yoga",
                    StartTime = now.Date.AddDays(1).AddHours(7),
                    EndTime = now.Date.AddDays(1).AddHours(8),
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
                    StartTime = now.Date.AddDays(1).AddHours(18),
                    EndTime = now.Date.AddDays(1).AddHours(19),
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
                    StartTime = now.Date.AddDays(2).AddHours(17),
                    EndTime = now.Date.AddDays(2).AddHours(18),
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
                    StartTime = now.Date.AddDays(3).AddHours(16),
                    EndTime = now.Date.AddDays(3).AddHours(17),
                    InstructorId = 2,
                    RoomId = 4,
                    CurrentCapacity = 5,
                    MaxCapacity = 18,
                    Status = TeamSessionStatus.Available
                }
            };

            await sessionCollection.InsertManyAsync(sessions);
        }

        // BOOKINGS
        var bookingsExist = await bookingCollection
            .Find(_ => true)
            .AnyAsync();

        if (!bookingsExist)
        {
            var bookings = new List<Booking>
            {
                new()
                {
                    BookingId = 1,
                    MemberId = 1,
                    SessionId = 1
                },

                new()
                {
                    BookingId = 2,
                    MemberId = 1,
                    SessionId = 3
                },

                new()
                {
                    BookingId = 3,
                    MemberId = 2,
                    SessionId = 2
                }
            };

            await bookingCollection.InsertManyAsync(bookings);
        }
    }
}