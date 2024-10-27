using System.Globalization;

public record Invoice
{
    private Guid _id;

    public string Id
    {
        get => _id.ToString().ToUpperInvariant();
        init => _id = Guid.Parse(value);
    }
    
    private Guid _userId;

    public string UserId
    {
        get => _userId.ToString().ToUpperInvariant();
        init => _userId = Guid.Parse(value);
    }

    public decimal Amount { get; init; }
}

public record User
{
    private Guid _id;

    public string Id
    {
        get => _id.ToString().ToUpperInvariant();
        init => _id = Guid.Parse(value);
    }

    public string FullName { get; init; }
}

public record Room
{
    private Guid _id;

    public string Id
    {
        get => _id.ToString().ToUpperInvariant();
        init => _id = Guid.Parse(value);
    }

    public decimal Price { get; init; }

    public string Name { get; init; }
}

public record Booking
{
    private Guid _userId;

    public string UserId
    {
        get => _userId.ToString().ToUpperInvariant();
        init => _userId = Guid.Parse(value);
    }

    private Guid _roomId;

    public string RoomId
    {
        get => _roomId.ToString().ToUpperInvariant();
        init => _roomId = Guid.Parse(value);
    }

    private DateOnly _date;
    public decimal Price { get; init; }

    public string Date
    {
        get => _date.ToString("yyyy-MM-dd");
        init => _date = DateOnly.Parse(value, CultureInfo.InvariantCulture);
    }

    private TimeSpan _start;

    public string Start
    {
        get => _start.ToString();
        init => _start = TimeSpan.Parse(value);
    }

    private TimeSpan _end;

    public string End
    {
        get => _end.ToString();
        init => _end = TimeSpan.Parse(value);
    }
}

public record Article
{
    
    private Guid _id;

    public string Id
    {
        get => _id.ToString().ToUpperInvariant();
        init => _id = Guid.Parse(value);
    }
    private Guid? _userId;

    public string? UserId
    {
        get => _userId?.ToString().ToUpperInvariant();
        init => _userId = value is null ? null : Guid.Parse(value);
    }
    
    public decimal Price { get; init; }
    public string Name { get; init; }
    
    public int Passed { get; init; }

}
