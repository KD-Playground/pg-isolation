using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;

namespace RoomBooker;

public static class BusinessLogic
{
    
    public static async Task CreateUserCommand(User user, SqliteConnection connexion)
    {
        var sql =
            """
                INSERT INTO Users (Id, FullName) VALUES (@Id, @FullName);
            """;

        var completedUser = user with {Id = Guid.NewGuid().ToString()};

        await connexion.ExecuteAsync(sql, completedUser);
    }
    
    public static async Task CreateRoomCommand(Room room, DbConnection connexion)
    {
        var sql =
            """
                INSERT INTO Rooms (Id, Name) VALUES (@Id, @Name);
            """;

        var completedRoom = room with {Id = Guid.NewGuid().ToString()};

        await connexion.ExecuteAsync(sql, completedRoom);
    }
    
    public static async Task<IResult> BookRoomCommand(DbConnection connexion, Booking booking, DbTransaction transaction)
    {
        try
        {
            var getRoom =
                """
                    SELECT * FROM Rooms Where Id = @Id LIMIT 1;
                """;
            var room = await connexion.QueryFirstOrDefaultAsync<Room>(getRoom, new {Id = booking.RoomId},
                transaction);

            if (room is null) throw new Exception("Room not found");

            var getUser =
                """
                   SELECT * FROM Users Where Id = @Id LIMIT 1;
                """;

            var user = await connexion.QueryFirstOrDefaultAsync<User>(getUser, new {Id = booking.UserId},
                transaction);

            if (user is null) throw new Exception("User not found");

            var getOverlappingBooking =
                """
                    SELECT * FROM Bookings 
                    WHERE UserId = @UserId 
                    AND RoomId = @RoomId
                    AND Date = @Date
                    AND ((Start <= @Start AND End >= @End) 
                            OR (Start >= @Start AND End <= @End)
                            OR (Start >= @Start AND End >= @End AND Start <= @End)
                            OR (Start <= @Start AND End <= @End AND End >= @Start))  
                """;

            var overlappingBookings =
                (await connexion.QueryAsync<Booking>(getOverlappingBooking, booking, transaction)).ToList();

            if (overlappingBookings.Any())
                return Results.BadRequest(new
                {
                    Message = "Cannot book room, because of overlapping bookings",
                    Data = overlappingBookings
                });

            var bookRoomForUser =
                """
                    INSERT INTO Bookings 
                    (RoomId, UserId, Date, Start, End, Price) 
                    VALUES (@RoomId, @UserId, @Date, @Start, @End, @Price);
                """;

            booking = booking with {Price = room.Price};

            await connexion.ExecuteAsync(bookRoomForUser, booking, transaction);

            await Task.Delay(TimeSpan.FromSeconds(30));

            var invoiceUser =
                """
                    INSERT INTO Invoices 
                    (Id, UserId, Amount) 
                    VALUES (@Id, @UserId, @Price);
                """;

            await connexion.ExecuteAsync(invoiceUser, new
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                room.Price
            }, transaction);

            await transaction.CommitAsync();

            return Results.Ok();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public static async Task<IResult> GetInventoryQuery(DbConnection connexion, DbTransaction transaction)
    {
        var getBookigns =
            """
                SELECT * FROM Bookings
            """;

        var bookings = await connexion.QueryAsync<Booking>(getBookigns, transaction);

        var getInvoices =
            """
                SELECT * FROM Invoices 
            """;

        var invoices = await connexion.QueryAsync<Invoice>(getInvoices, transaction);

        await transaction.CommitAsync();


        return Results.Ok(new
        {
            Invoices = invoices,
            Bookings = bookings
        });
    }
    
}