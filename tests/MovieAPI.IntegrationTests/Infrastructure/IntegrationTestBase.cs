using System.Net.Http.Json;

namespace MovieAPI.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase(IntegrationTestWebAppFactory factory) : IAsyncLifetime
{
  protected readonly HttpClient Client = factory.CreateClient();

  public Task InitializeAsync() => factory.ResetDatabaseAsync();

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
