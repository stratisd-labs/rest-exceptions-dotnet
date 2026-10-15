global using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace RestExceptions;

public class RestExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IEnumerable<IRestExceptionMapper> mappers,
    IRestExceptionProblemDetailsBuilder? problemDetailsBuilder = null,
    ILogger<RestExceptionHandler>? logger = null)
    : IExceptionHandler
{
    private const string ProblemJsonContentType = "application/problem+json";

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
        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        };
        if (await problemDetailsService.TryWriteAsync(problemDetailsContext))
        {
            return true;
        }

        // No writer accepts the request's Accept header (e.g. text/html), and returning false would make
        // ExceptionHandlerMiddleware fail with a 500. RFC 9457 allows a problem+json response regardless.
        await WriteProblemJsonAsync(problemDetailsContext, cancellationToken);
        return true;
    }

    private static async Task WriteProblemJsonAsync(
        ProblemDetailsContext problemDetailsContext,
        CancellationToken cancellationToken)
    {
        var httpContext = problemDetailsContext.HttpContext;
        var services = httpContext.RequestServices;

        // DefaultProblemDetailsWriter applies the customization only when it writes the response itself
        services.GetService<IOptions<ProblemDetailsOptions>>()?.Value.CustomizeProblemDetails?
            .Invoke(problemDetailsContext);

        var problemDetails = problemDetailsContext.ProblemDetails;
        var serializerOptions = services.GetService<IOptions<JsonOptions>>()?.Value.SerializerOptions
                                ?? JsonSerializerOptions.Web;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            problemDetails.GetType(),
            serializerOptions,
            ProblemJsonContentType,
            cancellationToken);
    }
}
