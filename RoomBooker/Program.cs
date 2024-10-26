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

#region v1 endpoints isolation read_uncomitted 

app.MapPost("/v1/users", Endpoints.CreateUser)
    .WithName("Create User")
    .WithOpenApi();

app.MapPost("/v1/rooms", Endpoints.CreateRoom)
    .WithName("Create Room")
    .WithOpenApi();

app.MapPost("/v1/articles", Endpoints.CreateArticle)
    .WithName("Create Article")
    .WithOpenApi();

app.MapPost("/v1/rooms/bookings", Endpoints.BookRoom)
    .WithName("Book Room")
    .WithOpenApi();

app.MapPost("/v1/rooms/fast-bookings", Endpoints.BookRoomFast)
    .WithName("Fast Book Room")
    .WithOpenApi();

app.MapPost("/v1/articles/sell", Endpoints.SellArticle)
    .WithName("Sell Article")
    .WithOpenApi();

app.MapPost("/v1/articles/fast-sell", Endpoints.SellFastArticle)
    .WithName("Sell Article Fast")
    .WithOpenApi();

app.MapPost("/v1/inventory", Endpoints.GetInventory)
    .WithName("Get Inventory")
    .WithOpenApi();

#endregion
// #region v2 endpoints isolation read_comitted
// app.MapPost("/v1/users", Endpoints.CreateUser)
//     .WithName("Create User")
//     .WithOpenApi();
//
// app.MapPost("/v1/rooms", Endpoints.CreateRoom)
//     .WithName("Create Room")
//     .WithOpenApi();
//
// app.MapPost("/v1/rooms/bookings", Endpoints.BookRoom)
//     .WithName("Book Room")
//     .WithOpenApi();
//
// app.MapPost("/v1/inventory", Endpoints.GetInventory)
//     .WithName("Get Inventory")
//     .WithOpenApi();
// #endregion


app.Run();

public partial class Program{}