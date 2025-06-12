using System.Text.Json;
using Cdv.Domain.DbContext;
using Cdv.Domain.Entities;
using Cdv.Functions.Dto;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cdv.Functions;

public class PeopleFn
{
    private readonly ILogger<PeopleFn> _logger;
    private readonly PeopleDbContext db;

    public PeopleFn(ILogger<PeopleFn> logger, PeopleDbContext db)
    {
        _logger = logger;
        this.db = db;
    }

    [Function("PeopleFn")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete")] HttpRequest req)
    {
        switch (req.Method)
        {
            case "POST":
                var person = await CreatePerson(req);
                return new OkObjectResult(person);
            case "GET":
                var idExist = req.Query.Any(x => x.Key == "Id");

                if (idExist)
                {
                    var personId = req.Query.First(x => x.Key == "Id").Value;
                    int id = Int32.Parse(personId.First());
                    return new OkObjectResult(FindPerson(id));
                }

                var people = GetPeople(req);
                return new OkObjectResult(people);
            case "DELETE":
                DeletePerson(req);
                return new OkResult();
            case "PUT":
                UpdatePerson(req);
                return new OkResult();
        }

        throw new NotImplementedException("Unknown method");
    }

    private List<PersonDto> GetPeople(HttpRequest req)
    {
        var people = db.People.ToList();
        return people.Select(s => new PersonDto
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName
        }).ToList();
    }
    private void UpdatePerson(HttpRequest req)
    {
        throw new NotImplementedException();
    }
    private void DeletePerson(HttpRequest req)
    {
        throw new NotImplementedException();
    }
    private PersonDto FindPerson(int personId)
    {
        var person = db.People.First(x => x.Id == personId);
        return new PersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName
        };
    }
    private async Task<PersonDto> CreatePerson(HttpRequest req)
    {
        using var streamReader = new StreamReader(req.Body);
        var requestBody = await streamReader.ReadToEndAsync();
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };


        var personDto = JsonSerializer.Deserialize<PersonDto>(requestBody, options);

        var person = new PersonEntity
        {
            FirstName = personDto.FirstName,
            LastName = personDto.LastName
        };
        
        db.People.Add(person);
        await db.SaveChangesAsync();
        
        return new PersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName
        };
    }
}