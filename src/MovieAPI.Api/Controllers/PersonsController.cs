using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Interfaces;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/people")]
public class PersonsController(IPersonService service) : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> GetPeople(string? name, string? genre, int? year,
    int? page, int? pageSize, CancellationToken cancellationToken)
  {

  }

  [HttpGet("{id}", Name = "GetPerson")]
  public async Task<IActionResult> GetPerson(Guid id, CancellationToken cancellationToken)
  {

  }
}
