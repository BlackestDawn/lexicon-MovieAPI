namespace MovieAPI.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
  public const string Name = "Integration";
}
