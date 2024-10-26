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
                    UserId TEXT NOT NULL,
                    Amount DECIMAL(20,0) NOT NULL,
                    PRIMARY KEY (Id),
                    FOREIGN KEY (UserId) REFERENCES Users (Id)
                );                       
            CREATE TABLE IF NOT EXISTS 
                "Rooms" (
                    Id Text NOT NULL,
                    Name Text NOT NULL,
                    Price DECIMAL(20,0) NOT NULL DEFAULT 200,
                    PRIMARY KEY (Id)
                );
            CREATE TABLE IF NOT EXISTS 
                "Bookings" (
                    RoomId Text NOT NULL,
                    UserId Text NOT NULL,
                    Date Text NOT NULL,
                    START Text NOT NULL,
                    END Test NOT NULL,
                    Price DECIMAL(20,0) NOT NULL DEFAULT 200,
                    FOREIGN KEY (RoomId) REFERENCES Rooms (Id),
                    FOREIGN KEY (UserId) REFERENCES Users (Id)
                );
            CREATE TABLE IF NOT EXISTS 
                "Articles" (
                    Id TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Price DECIMAL(20,0) NOT NULL DEFAULT 200,
                    UserId TEXT,
                    FOREIGN KEY (UserId) REFERENCES Users (Id)
                );
               
            DROP INDEX IF EXISTS idx_bookings_roomId_userId;
            CREATE INDEX IF NOT EXISTS idx_bookings_roomId_userId ON Bookings (RoomId, UserId);
            """;

        await connexion.ExecuteAsync(sql);
    };

    public static readonly Func<User, ILogger<Program>, Task> CreateUser = async (user, logger) =>
    {
        logger.LogInformation("Creating User...");
        await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
        await connexion.OpenAsync();

        await BusinessLogic.CreateUserCommand(user, connexion);
    };


    public static readonly Func<Room, ILogger<Program>, Task> CreateRoom = async (room, logger) =>
    {
        logger.LogInformation("Creating Room...");
        await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
        await connexion.OpenAsync();

        await BusinessLogic.CreateRoomCommand(room, connexion);
    };
    
    public static readonly Func<Article, ILogger<Program>, Task> CreateArticle = async (article, logger) =>
    {
        logger.LogInformation("Creating Article...");
        await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
        await connexion.OpenAsync();

        await BusinessLogic.CreateArticleCommand(article, connexion);
    };


    public static readonly Func<Booking, ILogger<Program>, Task<IResult>> BookRoom =
        async (booking, logger) =>
        {
            logger.LogInformation("Booking Room...");
            await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
            await connexion.OpenAsync();

            var sql = "PRAGMA read_uncommitted = ON;";

            await connexion.ExecuteAsync(sql);

            await using var transaction = connexion.BeginTransaction(IsolationLevel.ReadUncommitted);
            return await BusinessLogic.BookRoomCommand(connexion, booking, transaction, TimeSpan.FromSeconds(30));
        };
    
    public static readonly Func<Booking, ILogger<Program>, Task<IResult>> BookRoomFast =
        async (booking, logger) =>
        {
            logger.LogInformation("Booking Room...");
            await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
            await connexion.OpenAsync();

            var sql = "PRAGMA read_uncommitted = ON;";

            await connexion.ExecuteAsync(sql);

            await using var transaction = connexion.BeginTransaction(IsolationLevel.ReadUncommitted);
            return await BusinessLogic.BookRoomCommand(connexion, booking, transaction, TimeSpan.Zero);
        };
    
    public static readonly Func<Article, ILogger<Program>, Task<IResult>> SellArticle =
        async (article, logger) =>
        {
            logger.LogInformation("Selling Article...");
            await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
            await connexion.OpenAsync();

            var sql = "PRAGMA read_uncommitted = ON;";

            await connexion.ExecuteAsync(sql);

            await using var transaction = connexion.BeginTransaction(IsolationLevel.ReadUncommitted);
            return await BusinessLogic.SellArticleToUser(article, connexion, transaction, TimeSpan.FromSeconds(20));
        };
    
    public static readonly Func<Article, ILogger<Program>, Task<IResult>> SellFastArticle =
        async (article, logger) =>
        {
            logger.LogInformation("Selling Article Fast...");
            await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
            await connexion.OpenAsync();

            var sql = "PRAGMA read_uncommitted = ON;";

            await connexion.ExecuteAsync(sql);

            await using var transaction = connexion.BeginTransaction(IsolationLevel.ReadUncommitted);
            return await BusinessLogic.SellArticleToUser(article, connexion, transaction, TimeSpan.Zero);
        };


    public static readonly Func<ILogger<Program>, Task<IResult>> GetInventory = async (logger) =>
    {
        logger.LogInformation("Getting Inventory...");
        await using var connexion = new SqliteConnection(InMemoryConfiguration.SqlLiteConnectionString);
        await connexion.OpenAsync();

        var sql = "PRAGMA read_uncommitted = ON;";

        await connexion.ExecuteAsync(sql);

        await using var transaction = connexion.BeginTransaction(IsolationLevel.ReadUncommitted);

        return await BusinessLogic.GetInventoryQuery(connexion, transaction);
    };
}