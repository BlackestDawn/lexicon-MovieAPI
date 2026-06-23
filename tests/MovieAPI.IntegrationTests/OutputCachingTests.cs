using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure;
using MovieAPI.IntegrationTests.Infrastructure;

namespace MovieAPI.IntegrationTests;

public class OutputCachingTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
  [Fact]
  public async Task GetGenres_ServesStaleDataFromCache_UntilAnApiWriteEvictsIt()
  {
    var created = await CreateGenreAsync("Drama", "drama");

    var firstFetch = await Client.GetFromJsonAsync<List<GenreDto>>("/api/genres");
    Assert.Contains(firstFetch!, g => g.Name == "Drama");

    // Bypasses the API (and therefore the cache eviction it triggers) to prove the
    // second GET below is served from cache rather than hitting the database again.
    await RenameGenreDirectlyInDbAsync(created.Id, "Renamed Directly In DB");

    var cachedFetch = await Client.GetFromJsonAsync<List<GenreDto>>("/api/genres");
    Assert.Contains(cachedFetch!, g => g.Name == "Drama");
    Assert.DoesNotContain(cachedFetch!, g => g.Name == "Renamed Directly In DB");

    // Any write through the API evicts the shared "catalog" cache tag, even one
    // unrelated to genres.
    await CreateGenreAsync("Comedy", "comedy");

    var freshFetch = await Client.GetFromJsonAsync<List<GenreDto>>("/api/genres");
    Assert.Contains(freshFetch!, g => g.Name == "Renamed Directly In DB");
  }

  [Fact]
  public async Task GetMovies_ReflectsGenreRename_AfterGenreUpdateEvictsSharedCache()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    await CreateMovieAsync(genreId, personId);

    var firstFetch = await Client.GetFromJsonAsync<List<MovieDto>>("/api/movies");
    Assert.Contains(firstFetch!.Single().Genres, g => g.Name == "Sci-Fi");

    var response = await Client.PutAsJsonAsync($"/api/genres/{genreId}", TestData.ValidGenre("Period Drama", "period-drama"));
    response.EnsureSuccessStatusCode();

    var secondFetch = await Client.GetFromJsonAsync<List<MovieDto>>("/api/movies");
    Assert.Contains(secondFetch!.Single().Genres, g => g.Name == "Period Drama");
  }

  [Fact]
  public async Task GetMovie_AverageRating_UpdatesAfterEachReviewEvictsSharedCache()
  {
    var (genreId, personId) = await CreateGenreAndPersonAsync();
    var movie = await CreateMovieAsync(genreId, personId);

    var afterCreate = await Client.GetFromJsonAsync<MovieExtendedDto>($"/api/movies/{movie.Id}");
    Assert.Equal(0, afterCreate!.AverageRating);

    await Client.PostAsJsonAsync($"/api/movies/{movie.Id}/reviews", TestData.ValidReview(score: 8));
    var afterFirstReview = await Client.GetFromJsonAsync<MovieExtendedDto>($"/api/movies/{movie.Id}");
    Assert.Equal(8, afterFirstReview!.AverageRating);

    await Client.PostAsJsonAsync($"/api/movies/{movie.Id}/reviews", TestData.ValidReview("Second Reviewer", 4));
    var afterSecondReview = await Client.GetFromJsonAsync<MovieExtendedDto>($"/api/movies/{movie.Id}");
    Assert.Equal(6, afterSecondReview!.AverageRating);
  }

  private async Task RenameGenreDirectlyInDbAsync(Guid id, string newName)
  {
    using var scope = Factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var genre = await db.Genres.SingleAsync(g => g.Id == id);
    genre.Name = newName;
    await db.SaveChangesAsync();
  }

  private async Task<GenreDto> CreateGenreAsync(string name, string slug)
  {
    var response = await Client.PostAsJsonAsync("/api/genres", TestData.ValidGenre(name, slug));
    return (await response.Content.ReadFromJsonAsync<GenreDto>())!;
  }

  private async Task<(Guid GenreId, Guid PersonId)> CreateGenreAndPersonAsync()
  {
    var genreResponse = await Client.PostAsJsonAsync("/api/genres", TestData.ValidGenre());
    var genre = (await genreResponse.Content.ReadFromJsonAsync<GenreDto>())!;

    var personResponse = await Client.PostAsJsonAsync("/api/people", TestData.ValidPerson());
    var person = (await personResponse.Content.ReadFromJsonAsync<PersonDto>())!;

    return (genre.Id, person.Id);
  }

  private async Task<MovieDto> CreateMovieAsync(Guid genreId, Guid personId)
  {
    var response = await Client.PostAsJsonAsync("/api/movies", TestData.ValidMovie(genreId, personId));
    return (await response.Content.ReadFromJsonAsync<MovieDto>())!;
  }
}
