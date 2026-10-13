global using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RestExceptions;

public class RestExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IEnumerable<IRestExceptionMapper> mappers,
    IRestExceptionProblemDetailsBuilder? problemDetailsBuilder = null,
    ILogger<RestExceptionHandler>? logger = null)
    : IExceptionHandler
{
    private readonly ILogger _logger = logger ?? NullLogger<RestExceptionHandler>.Instance;
    private readonly IRestExceptionMapper[] _mappers = mappers.ToArray();
    private readonly IRestExceptionProblemDetailsBuilder _problemDetailsBuilder =
        problemDetailsBuilder ?? new DefaultRestExceptionProblemDetailsBuilder();

    public RestExceptionHandler(
        IProblemDetailsService problemDetailsService,
        IRestExceptionProblemDetailsBuilder? problemDetailsBuilder = null)
        : this(problemDetailsService, [], problemDetailsBuilder)
    {
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var restException = exception.MapToRestException(_mappers);

        // Since .NET 10, ExceptionHandlerMiddleware no longer logs exceptions handled by an IExceptionHandler
        if ((int)restException.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception has occurred while executing the request.");
        }

        var problemDetails = _problemDetailsBuilder.Build(httpContext, restException);

        // Add the extensions from the RestException to the ProblemDetails
        foreach (var kvp in restException.Extensions)
        {
            if (kvp.Value is null)
            {
                continue;
            }

            problemDetails.Extensions[kvp.Key] = kvp.Value;
        }

        httpContext.Response.StatusCode = (int)restException.StatusCode;
        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
    }
}
