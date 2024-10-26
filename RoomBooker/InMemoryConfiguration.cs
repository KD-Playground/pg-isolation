namespace RoomBooker;

public static class InMemoryConfiguration
{
    public const string PgSqlConnectionString =
        "Host=localhost;Port=15432;Username=admin;Password=admin;Database=RoomBooker";
    public const string SqlLiteConnectionString =
        "Data Source=lite.db;cache=shared";
}