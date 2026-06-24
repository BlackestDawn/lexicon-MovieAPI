using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using MovieAPI.Domain.Constants;

namespace MovieAPI.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase(IntegrationTestWebAppFactory factory) : IAsyncLifetime
{
  protected readonly IntegrationTestWebAppFactory Factory = factory;
  protected readonly HttpClient Client = factory.CreateClient();

  public async Task InitializeAsync()
  {
    await Factory.ResetDatabaseAsync();

    // The output cache store is a singleton shared by every test in this collection,
    // and only the database gets reset between tests, so a cached response from a
    // previous test could otherwise leak into this one.
    using var scope = Factory.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<IOutputCacheStore>()
      .EvictByTagAsync("catalog", CancellationToken.None);

    // Most existing tests exercise plain CRUD round-trips, not authorization rules,
    // so the shared client defaults to full access. Tests that specifically assert
    // role boundaries build their own client with a lower-privileged token instead.
    var token = await Factory.CreateUserTokenAsync(Roles.Administrator);
    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
  }

  public Task DisposeAsync() => Task.CompletedTask;

  // A fresh client carrying its own token, for tests that need a specific role or
  // a different user identity than the shared Administrator-by-default Client.
  protected async Task<HttpClient> CreateClientWithRoleAsync(string role)
  {
    var client = Factory.CreateClient();
    var token = await Factory.CreateUserTokenAsync(role);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    return client;
  }

  // The JSON Patch input formatter only matches application/json-patch+json,
  // unlike PatchAsJsonAsync which sends the plain application/json content type.
  protected Task<HttpResponseMessage> PatchJsonPatchAsync(string requestUri, object patchDocument) =>
    PatchJsonPatchAsync(Client, requestUri, patchDocument);

  protected static Task<HttpResponseMessage> PatchJsonPatchAsync(HttpClient client, string requestUri, object patchDocument)
  {
    var content = JsonContent.Create(patchDocument);
    content.Headers.ContentType!.MediaType = "application/json-patch+json";
    return client.PatchAsync(requestUri, content);
  }
}
