using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

var factory = new HotelContextFactory();
using var context = factory.CreateDbContext();
if (args.Length > 0) await AddData();
await QueringData();

async Task AddData()
{
    Console.Write("How many hotels you want to add?: ");
    string userInput = Console.ReadLine();
    int hotelCount;
    bool success = Int32.TryParse(userInput, out hotelCount);
    if (!success || hotelCount < 1) throw new Exception("Input must be a number above 0");
    List<Hotel> hotels = GetHotels(hotelCount);

    Console.WriteLine();
    Console.WriteLine();

    for (int i = 0; i < hotels.Count; i++)
    {
        Console.Write($"How many rooms should the {i+1}. hotel have?: ");
        userInput = Console.ReadLine();
        int roomCount;
        success = Int32.TryParse(userInput, out roomCount);
        if (!success || roomCount < 0) throw new Exception("Input must be positive");
        hotels = GetRooms(roomCount, hotels, i);
    }
    

    await context.AddRangeAsync(hotels);
    await context.SaveChangesAsync();
}
async Task QueringData()
{
    int indexer = 0;
    var hotels = await context.Hotels
        .ToListAsync();
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    foreach (var item in hotels)
    {
        var rooms = await context.Rooms
            .Where(r => r.HotelId == item.Id)
            .ToListAsync();
        Console.WriteLine($"# {item.Name}");
        Console.WriteLine();
        Console.WriteLine("## Location");
        Console.WriteLine();
        Console.WriteLine($"{item.Street}");
        Console.WriteLine($"{item.ZIPCode} {item.City}");
        Console.WriteLine();
        Console.WriteLine("## Specials");
        Console.WriteLine();
        foreach (var special in item.Specials)
        {
            Console.WriteLine($"* {special.Peculiarity}");
        }
        Console.WriteLine();
        Console.WriteLine($"## Room Types");
        Console.WriteLine();
        Console.WriteLine("| Room Type | Size | Price Valid From | Price Valid To | Price in € |");
        Console.WriteLine("| --------- | ---- | ---------------- | -------------- | ---------- |");
        foreach (var room in rooms)
        {
            Console.WriteLine($"| {room.Titel} | {room.Size} | {item.Rooms[indexer].From} | {item.Rooms[indexer].To} | {item.Rooms[indexer].PriceEur} |");
        }
        Console.WriteLine();
        indexer++;
    }
}

List<Hotel> GetHotels(int count)
{
    List<Hotel> hotels = new();
    for (int i = 0; i < count; i++)
    {
        Console.WriteLine();
        Console.Write("Hotel name?: ");
        string hotelName = Console.ReadLine();
        Console.Write("Street?: ");
        string street = Console.ReadLine();
        Console.Write("ZIPCode?: ");
        string zipCode = Console.ReadLine();
        Console.Write("City?: ");
        string city = Console.ReadLine();
        Console.Write("How many specials should the hotel have?: ");
        string userInput = Console.ReadLine();
        int specialsCount;
        bool success = Int32.TryParse(userInput, out specialsCount);
        if (!success || specialsCount < 0) throw new Exception("Input must be positive");
        List<Special> specials = new List<Special>();
        for (int j = 0; j < specialsCount; j++)
        {
            Console.Write($"{j+1}. special?: ");
            userInput = Console.ReadLine();
            specials.Add(new Special { Peculiarity = userInput });
        }
        Hotel newHotel = new Hotel
        {
            Name = hotelName,
            Street = street,
            ZIPCode = zipCode,
            City = city,
            Specials = specials
        };
        hotels.Add(newHotel);
    }
    return hotels;
}

List<Hotel> GetRooms(int count, List<Hotel> hotels, int hotelIndex)
{
    //Dictionary<int, List<Room>> roomAsignment = new();
    List<Room> rooms = new();
    for (int j = 0; j < count; j++)
    {
        Console.WriteLine($"{j+1}. Room");
        Console.Write("Titel?: ");
        string titel = Console.ReadLine();
        Console.Write("Size?: ");
        string size = Console.ReadLine();
        Console.Write("Price per night?: ");
        string userInput = Console.ReadLine();
        decimal price;
        bool success = Decimal.TryParse(userInput, out price);
        if (!success || price < 0) throw new Exception("Input must be positive");
        Console.Write("Price valid from?: ");
        userInput = Console.ReadLine();
        bool fromNull = string.IsNullOrEmpty(userInput);
        DateTime from = new DateTime();
        if (!fromNull)
        {
            success = DateTime.TryParse(userInput, out from);
            if (!success) throw new Exception("Invalid input");
        }
        Console.Write("Price valid to?: ");
        userInput = Console.ReadLine();
        bool toNull = string.IsNullOrEmpty(userInput);
        DateTime to = new DateTime();
        if (!toNull)
        {
            success = DateTime.TryParse(userInput, out to);
            if (!success) throw new Exception("Invalid input");
        }
        Console.Write("DisabilityAccess? [true/false]: ");
        userInput = Console.ReadLine();
        bool disability = false;
        if (userInput.ToLower().Equals("true")) disability = true;
        Room newRoom = new Room
        {
            Titel = titel,
            Size = size,
            PriceEur = price,
            From = fromNull ? null : from,
            To = toNull ? null : to,
            DisabilityAccess = disability
        };
        rooms.Add(newRoom);
        Console.WriteLine();
    }
    hotels[hotelIndex].Rooms = rooms;
    return hotels;
}

#region Model
class Hotel
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Street { get; set; } = string.Empty;

    [MaxLength(10)]
    public string ZIPCode { get; set; } = string.Empty;

    [MaxLength(30)]
    public string City { get; set; } = string.Empty;

    public List<Special>? Specials { get; set; } = new();

    public List<Room>? Rooms { get; set; }
}

class Special
{
    public int Id { get; set; }

    [MaxLength(40)]
    public string Peculiarity { get; set; } = string.Empty;

    public List<Hotel>? Hotels { get; set; } = new();
}

class RoomAdditional
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Titel { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(15)]
    public string Size { get; set; }

    public bool DisabilityAccess { get; set; }

    public Hotel Hotel { get; set; }

    public int HotelId { get; set; }
}

class Room : RoomAdditional
{
    [Column(TypeName = "date")]
    public DateTime? From { get; set; }

    [Column(TypeName = "date")]
    public DateTime? To { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal PriceEur { get; set; }
}
#endregion

#region DbContext
class HotelContext : DbContext
{
    public HotelContext(DbContextOptions<HotelContext> options)
        : base(options)
    { }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Special> Specials { get; set; }
    public DbSet<RoomAdditional> Rooms { get; set; }
    public DbSet<Room> RoomPrices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>().HasBaseType<RoomAdditional>();
    }
}

class HotelContextFactory : IDesignTimeDbContextFactory<HotelContext>
{
    public HotelContext CreateDbContext(string[]? args = null)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<HotelContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new HotelContext(optionsBuilder.Options);
    }
}
#endregion
