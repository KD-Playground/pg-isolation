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

    public static async Task CreateArticleCommand(Article article, SqliteConnection connexion)
    {
        var sql =
            """
                INSERT INTO Articles (Id, Price, Name) VALUES (@Id, @Price, @Name);
            """;

        var compltedArticle = article with {Id = Guid.NewGuid().ToString()};

        await connexion.ExecuteAsync(sql, compltedArticle);
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

    public static async Task<IResult> BookRoomCommand(DbConnection connexion, Booking booking,
        DbTransaction transaction, TimeSpan delay)
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

            // var getOverlappingBooking =
            //     """
            //         SELECT * FROM Bookings 
            //         WHERE RoomId = @RoomId
            //         AND Date = @Date
            //         AND ((Start <= @Start AND End >= @End) 
            //                 OR (Start >= @Start AND End <= @End)
            //                 OR (Start >= @Start AND End >= @End AND Start <= @End)
            //                 OR (Start <= @Start AND End <= @End AND End >= @Start))  
            //     """;
            //
            // var overlappingBookings =
            //     (await connexion.QueryAsync<Booking>(getOverlappingBooking, booking, transaction)).ToList();
            //
            // if (overlappingBookings.Any())
            //     return Results.BadRequest(new
            //     {
            //         Message = "Cannot book room, because of overlapping bookings",
            //         Data = overlappingBookings
            //     });

            var bookRoomForUser =
                """
                    INSERT INTO Bookings 
                    (RoomId, UserId, Date, Start, End, Price) 
                    VALUES (@RoomId, @UserId, @Date, @Start, @End, @Price);
                """;

            booking = booking with {Price = room.Price};

            await connexion.ExecuteAsync(bookRoomForUser, booking, transaction);

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

    public static async Task<IResult> SellArticleToUser(Article article, DbConnection connexion,
        DbTransaction transaction, TimeSpan delay)
    {
        try
        {
            var getArticle =
                """
                    SELECT * FROM Articles Where Id = @Id LIMIT 1;
                """;
            var actualArticle = await connexion.QueryFirstOrDefaultAsync<Article>(getArticle, new {Id = article.Id},
                transaction);

            if (actualArticle is null) throw new Exception("Article not found");
            if (actualArticle.UserId is not null) throw new Exception("Article is Sold");

            var getUser =
                """
                   SELECT * FROM Users Where Id = @Id LIMIT 1;
                """;

            var user = await connexion.QueryFirstOrDefaultAsync<User>(getUser, new {Id = article.UserId},
                transaction);

            if (user is null) throw new Exception("User not found");


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
                actualArticle.Price
            }, transaction);

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay);

            var sellArticle =
                """
                    UPDATE Articles 
                    Set UserId = @UserId
                    Where Id = @Id;
                """;

            await connexion.ExecuteAsync(sellArticle, new {actualArticle.Id, UserId = user.Id}, transaction);

            await transaction.CommitAsync();


            return Results.Ok();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public static async Task<IResult> GetInventoryQuery(DbConnection connexion, DbTransaction transaction, ILogger<Program> logger)
    {
        var getBookigns =
            """
                SELECT * FROM Bookings
            """;

        var bookings = (await connexion.QueryAsync<Booking>(getBookigns, transaction)).ToList();

        var getInvoices =
            """
                SELECT * FROM Invoices 
            """;

        var invoices = (await connexion.QueryAsync<Invoice>(getInvoices, transaction)).ToList();

        await transaction.CommitAsync();

        var isValid = invoices.Sum(i => i.Amount) == bookings.Sum(i => i.Price);


        return Results.Ok(new
        {
            Invoices = invoices,
            Bookings = bookings,
            IsValid = isValid
        });
    }
}