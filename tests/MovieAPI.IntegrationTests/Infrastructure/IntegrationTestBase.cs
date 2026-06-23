using System.Net.Http.Json;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;

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
  }

  public Task DisposeAsync() => Task.CompletedTask;

  // The JSON Patch input formatter only matches application/json-patch+json,
  // unlike PatchAsJsonAsync which sends the plain application/json content type.
  protected Task<HttpResponseMessage> PatchJsonPatchAsync(string requestUri, object patchDocument)
  {
    var content = JsonContent.Create(patchDocument);
    content.Headers.ContentType!.MediaType = "application/json-patch+json";
    return Client.PatchAsync(requestUri, content);
  }
}
