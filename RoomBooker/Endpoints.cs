using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace RoomBooker;

public static class Endpoints
{
    public static readonly Func<ILogger<Program>, Task> PgSqlMigrate = async (logger) =>
    {
        logger.LogInformation("Migrating database...");
        await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
        await connexion.OpenAsync();
    };

    public static readonly Func<ILogger<Program>, Task> SqlLiteMigrate = async (logger) =>
    {
        logger.LogInformation("Migrating database...");
        await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
        await connexion.OpenAsync();

        var sql =
            """
            PRAGMA foreign_keys = ON;
            PRAGMA read_uncommitted = ON;

            CREATE TABLE IF NOT EXISTS 
                "Users" (
                    Id TEXT NOT NULL,
                    FullName DECIMAL(20,0) NOT NULL,
                    PRIMARY KEY (Id)
                );                       

            CREATE TABLE IF NOT EXISTS 
                "Invoices" (
                    Id TEXT NOT NULL,
                    Amount DECIMAL(20,0) NOT NULL,
                    PRIMARY KEY (Id)
                );                       
            CREATE TABLE IF NOT EXISTS 
                "Rooms" (
                    Id Text NOT NULL,
                    Name Text NOT NULL
                );
            CREATE TABLE IF NOT EXISTS 
                "Bookings" (
                    RoomId Text NOT NULL,
                    UserId Text NOT NULL,
                    Date Text NOT NULL,
                    START Text NOT NULL,
                    END Test NOT NULL,
                    FOREIGN KEY (RoomId) REFERENCES Rooms (Id)
                    FOREIGN KEY (UserId) REFERENCES Users (Id)
                );
            CREATE UNIQUE INDEX idx_bookings_roomId_userId ON Bookings (RoomId, UserId);
            """;

        await connexion.ExecuteAsync(sql);
    };

    public static readonly Func<User, ILogger<Program>, Task> CreateUser = async (user, logger) =>
    {
        logger.LogInformation("Creating User...");
        await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
        await connexion.OpenAsync();

        var sql =
            """
                INSERT INTO Users (Id, FullName) VALUES (@Id, @FullName);
            """;

        var completedUser = user with {Id = Guid.NewGuid()};

        await connexion.ExecuteAsync(sql, completedUser);
    };
    
    public static readonly Func<Room, ILogger<Program>, Task> CreateRoom = async (room, logger) =>
    {
        logger.LogInformation("Creating Room...");
        await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
        await connexion.OpenAsync();

        var sql =
            """
                INSERT INTO Rooms (Id, Name) VALUES (@Id, @Name);
            """;

        var completedRoom = room with {Id = Guid.NewGuid()};

        await connexion.ExecuteAsync(sql, completedRoom);
    };
    
    public static readonly Func<Booking, ILogger<Program>, Task> BookRoom = async (booking, logger) =>
    {
        logger.LogInformation("Booking Room...");
        await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
        await connexion.OpenAsync();
        
        // TODO - book the room
        // calculator process checks that room and invoice total is not aligned
        // create an invoice

        connexion.BeginTransaction(IsolationLevel.ReadUncommitted);

        var sql =
            """
                INSERT INTO Bookings (RoomId, UserId, Date, Start, End) VALUES (@RoomId, @UserId, @Date, @Start, @End);  
            """;

        await connexion.ExecuteAsync(sql, booking);
    };
}

public record Invoice
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
}

public record User
{
    public Guid Id { get; init; }
    public string FullName { get; init; }
}

public record Room
{
    public Guid Id { get; init; }
    public string Name { get; init; }
}

public record Booking
{
    public Guid UserId { get; init; }
    public Guid RoomId { get; init; }
    public DateOnly Date { get; init; }
    public TimeSpan Start { get; init; }
    public TimeSpan End { get; init; }
}