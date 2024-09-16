using FindTheMole.Hubs;
using FindTheMole.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("angularApp", pb =>
    {
        pb.WithOrigins("http://localhost:4200");
        pb.AllowAnyHeader();
        pb.AllowAnyMethod();
        pb.AllowCredentials();
    });
}
);
builder.Services.AddSignalR();

builder.Services.AddSingleton<ICollection<Message>>(new List<Message>());
builder.Services.AddSingleton<ICollection<Game>>(new List<Game>());
builder.Services.AddSingleton<ICollection<Player>>(new List<Player>());
builder.Services.AddSingleton<ICollection<string>>(new HashSet<string>
{
    "Airplane", "Bank", "Beach", "Hospital", "School",
    "Restaurant", "Train", "Supermarket", "Football Stadium",
    "Park", "Cinema", "Hotel", "Outer Space", "Museum", "Boat"
});

var app = builder.Build();

app.UseCors("angularApp");

app.UseHttpsRedirection();

app.MapHub<GameHub>("/game");

app.MapControllers();

app.Run();
