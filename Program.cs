using FindTheMole.Hubs;
using FindTheMole.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("angularApp", pb =>
    {
        pb.WithOrigins("https://selimbamri.github.io");
        pb.AllowAnyHeader();
        pb.AllowAnyMethod();
        pb.AllowCredentials();
    });
}
);
builder.Services.AddSignalR();

builder.Services.AddSingleton<ICollection<Message>>(new HashSet<Message>());
builder.Services.AddSingleton<ICollection<Game>>(new HashSet<Game>());
builder.Services.AddSingleton<ICollection<Player>>(new HashSet<Player>());
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
