using System.Text.Json;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Interfaces;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController(IMovieService service) : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> GetMovies(string? name, string? search, string? genre,
    int? year, decimal? minRating,
    int? page, int? pageSize,
    CancellationToken cancellationToken = default)
  {
    var (result, pagination) = await service.GetMany(
      new MovieSearchParams(name, search, genre, year, minRating),
      page, pageSize, cancellationToken);

    if (pagination != null)
    {
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
    }

    return Ok(result);
  }

  [HttpGet("{id}", Name = "GetMovie")]
  public async Task<IActionResult> GetMovie(Guid id, bool includePeople = true,
    CancellationToken cancellationToken = default)
  {
    var result = await service.GetOne(id, includePeople, cancellationToken);

    if (result == null)
    {
      return NotFound();
    }

    return Ok(result);
  }

  [HttpPost]
  public async Task<IActionResult> CreateMovie(MovieForCreationDto newMovie,
    CancellationToken cancellationToken = default)
  {
    var result = await service.Create(newMovie, cancellationToken);

    if (!result.Success)
    {
      return BadRequest(result.ErrorMessage);
    }

    return CreatedAtRoute("GetMovie", new { result.Movie!.Id }, result.Movie);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateMovie(Guid id, MovieForUpdateDto updatedMovie,
    CancellationToken cancellationToken)
  {
    var (success, message) = await service.Update(id, updatedMovie, cancellationToken);

    if (!success)
    {
      return BadRequest(message);
    }

    return NoContent();
  }

  [HttpPatch("{id}")]
  public async Task<IActionResult> PatchMovie(Guid id, JsonPatchDocument patch,
    CancellationToken cancellationToken)
  {
    var (success, message) = await service.Update(id, patch, cancellationToken);

    if (!success)
    {
      return BadRequest(message);
    }

    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> RemoveMovie(Guid id,
    CancellationToken cancellationToken)
  {
    await service.Remove(id, cancellationToken);
    return NoContent();
  }
}
