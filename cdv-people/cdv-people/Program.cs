using cdv_people.Db;
using cdv_people.Db.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PeopleDb>(options =>
{
    var cs = builder.Configuration.GetConnectionString("Db");
    options.UseSqlServer(cs);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/people", (PeopleDb db) =>
    {
        var people = db.People.ToList();
    return people;
})
.WithName("GetPeople");

app.MapPost("/people", (PeopleDb db, PersonEntity person) =>
    {
        db.People.Add(person);
        db.SaveChanges();

    })
    .WithName("AddPerson");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
