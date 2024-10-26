using Npgsql;
using RoomBooker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/v2/migrate", Endpoints.PgSqlMigrate)
    .WithName("Pg SQL Migrate")
    .WithOpenApi();

app.MapGet("/v1/migrate", Endpoints.SqlLiteMigrate)
    .WithName("SQL lite Migrate")
    .WithOpenApi();


app.MapPost("/v1/users", Endpoints.CreateUser)
    .WithName("Create User")
    .WithOpenApi();

app.MapPost("/v1/rooms", Endpoints.CreateRoom)
    .WithName("Create Room")
    .WithOpenApi();

app.MapPost("/v1/rooms/bookings", Endpoints.BookRoom)
    .WithName("Book Room")
    .WithOpenApi();

app.Run();

public partial class Program{}