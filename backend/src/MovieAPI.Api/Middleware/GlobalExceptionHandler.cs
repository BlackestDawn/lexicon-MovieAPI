using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Application.Exceptions;

namespace MovieAPI.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
  {
    var (statusCode, title) = exception switch
    {
      NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
      AuthenticationException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
      ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
      ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
      _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
    };

    if (statusCode == StatusCodes.Status500InternalServerError)
    {
      logger.LogError(exception, "Unhandled exception");
    }

    httpContext.Response.StatusCode = statusCode;

    await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
    {
      Status = statusCode,
      Title = title,
      Detail = exception.Message,
    }, cancellationToken);

    return true;
  }
}
