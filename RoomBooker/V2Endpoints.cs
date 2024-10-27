using System.Data;
using Dapper;
using Npgsql;

namespace RoomBooker;

public static class V2Endpoints
{
    private const IsolationLevel ISOLATION = IsolationLevel.RepeatableRead;

    public static readonly Func<ILogger<Program>, Task> PgSqlMigrate = async (logger) =>
    {
        logger.LogInformation("Migrating database...");
        await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
        await connexion.OpenAsync();

        var sql =
            """
            CREATE TABLE IF NOT EXISTS 
                "Users" (
                    Id TEXT NOT NULL,
                    FullName Text NOT NULL,
                    PRIMARY KEY (Id)
                );                       

            CREATE TABLE IF NOT EXISTS 
                "Invoices" (
                    Id TEXT NOT NULL,
                    UserId TEXT NOT NULL,
                    Amount DECIMAL(20,0) NOT NULL,
                    PRIMARY KEY (Id),
                    FOREIGN KEY (UserId) REFERENCES "Users" (Id)
                );                       
            CREATE TABLE IF NOT EXISTS 
                "Rooms" (
                    Id TEXT NOT NULL,
                    Name Text NOT NULL,
                    Price DECIMAL(20,0) NOT NULL DEFAULT 200,
                    PRIMARY KEY (Id)
                );
            CREATE TABLE IF NOT EXISTS 
                "Bookings" (
                    RoomId TEXT NOT NULL,
                    UserId TEXT NOT NULL,
                    Date Text NOT NULL,
                    START Text NOT NULL,
                    "End" Text NOT NULL,
                    Price DECIMAL(20,0) NOT NULL DEFAULT 200,
                    FOREIGN KEY (RoomId) REFERENCES "Rooms" (Id),
                    FOREIGN KEY (UserId) REFERENCES "Users" (Id)
                );
            CREATE TABLE IF NOT EXISTS 
                "Articles" (
                    Id TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Price DECIMAL(20,0) NOT NULL DEFAULT 200,
                    UserId TEXT,
                    FOREIGN KEY (UserId) REFERENCES "Users" (Id)
                );
               
            DROP INDEX IF EXISTS idx_bookings_roomId_userId;
            CREATE INDEX IF NOT EXISTS idx_bookings_roomId_userId ON "Bookings" (RoomId, UserId);

            ALTER TABLE "Articles" ADD COLUMN IF NOT EXISTS Passed INTEGER NOT NULL DEFAULT 0; 
            """;

        await connexion.ExecuteAsync(sql);
    };

    public static readonly Func<User, ILogger<Program>, Task> CreateUser = async (user, logger) =>
    {
        logger.LogInformation("Creating User...");
        await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
        await connexion.OpenAsync();

        await BusinessLogic.CreateUserCommand(user, connexion);
    };


    public static readonly Func<Room, ILogger<Program>, Task> CreateRoom = async (room, logger) =>
    {
        logger.LogInformation("Creating Room...");
        await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
        await connexion.OpenAsync();

        await BusinessLogic.CreateRoomCommand(room, connexion);
    };
    
    public static readonly Func<Article, ILogger<Program>, Task> CreateArticle = async (article, logger) =>
    {
        logger.LogInformation("Creating Article...");
        await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
        await connexion.OpenAsync();

        await BusinessLogic.CreateArticleCommand(article, connexion);
    };


    public static readonly Func<Booking, ILogger<Program>, Task<IResult>> BookRoom =
        async (booking, logger) =>
        {
            logger.LogInformation("Booking Room...");
            await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
            await connexion.OpenAsync();

            await using var transaction = connexion.BeginTransaction(ISOLATION);
            return await BusinessLogic.BookRoomCommand(connexion, booking, transaction, TimeSpan.FromSeconds(30));
        };
    
    public static readonly Func<Booking, ILogger<Program>, Task<IResult>> BookRoomFast =
        async (booking, logger) =>
        {
            logger.LogInformation("Booking Room...");
            await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
            await connexion.OpenAsync();

            await using var transaction = connexion.BeginTransaction(ISOLATION);
            return await BusinessLogic.BookRoomCommand(connexion, booking, transaction, TimeSpan.Zero);
        };
    
    public static readonly Func<Article, ILogger<Program>, Task<IResult>> SellArticle =
        async (article, logger) =>
        {
            logger.LogInformation("Selling Article...");
            await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
            await connexion.OpenAsync();


            await using var transaction = connexion.BeginTransaction(ISOLATION);
            return await BusinessLogic.SellArticleToUserWithPassed(article, connexion, transaction, TimeSpan.FromSeconds(20));
        };
    
    public static readonly Func<Article, ILogger<Program>, Task<IResult>> SellFastArticle =
        async (article, logger) =>
        {
            logger.LogInformation("Selling Article Fast...");
            await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
            await connexion.OpenAsync();

            await using var transaction = connexion.BeginTransaction(ISOLATION);
            return await BusinessLogic.SellArticleToUser(article, connexion, transaction, TimeSpan.Zero);
        };


    public static readonly Func<ILogger<Program>, Task<IResult>> GetInventory = async (logger) =>
    {
        logger.LogInformation("Getting Inventory...");
        await using var connexion = new NpgsqlConnection(InMemoryConfiguration.PgSqlConnectionString);
        await connexion.OpenAsync();

        await using var transaction = connexion.BeginTransaction(ISOLATION);

        return await BusinessLogic.GetInventoryQuery(connexion, transaction, logger);
    };
}