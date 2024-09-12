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

var app = builder.Build();

app.UseCors("angularApp");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
