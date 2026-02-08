using MeetingIntelli.Endpoints;
using MeetingIntelli.Extension;
using MeetingIntelli.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddAppServices(builder.Configuration);


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "MeetingIntelli")
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();


builder.Host.UseSerilog();
builder.Services.AddCorsPolicy();
// Register browser pool as singleton (one instance for the entire app)
builder.Services.AddSingleton<IBrowserPool, BrowserPool>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.SeedDataAsync();
    app.MapOpenApi();
}



//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors("AllowFrontend");
app.MapMeetingEndpoints();
app.Run();
