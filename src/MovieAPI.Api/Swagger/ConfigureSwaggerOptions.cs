using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MovieAPI.Api.Swagger;

// Swashbuckle has no built-in notion of API versions, so without this it only ever
// produces a single implicit "v1" document covering every controller. This adds one
// SwaggerDoc per version reported by Asp.Versioning, keyed by the same GroupName
// UseSwaggerUI iterates over via app.DescribeApiVersions().
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
  public void Configure(SwaggerGenOptions options)
  {
    foreach (var description in provider.ApiVersionDescriptions)
    {
      options.SwaggerDoc(description.GroupName, new OpenApiInfo
      {
        Title = "MovieAPI",
        Version = description.ApiVersion.ToString(),
      });
    }
  }
}
