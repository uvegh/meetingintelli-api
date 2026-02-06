using MeetingIntelli.Extension;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAppServices(builder.Configuration);

builder.Services.AddCorsPolicy();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.SeedDataAsync();
    app.MapOpenApi();
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
