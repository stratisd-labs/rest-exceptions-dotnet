using FluentValidation;
using FluentValidation.Results;

namespace RestExceptions.FluentValidation;

/// <summary>
/// Maps FluentValidation's <see cref="ValidationException"/> to a <see cref="RestException"/>.
/// </summary>
/// <remarks>
/// The status code is resolved from each failure's <see cref="ValidationFailure.ErrorCode"/>
/// (see <see cref="RestExceptionFactory.TryResolveStatusCode"/>), e.g.
/// <code>
/// RuleFor(x => x.Name)
///     .WithErrorCode("409")                              // 409 Conflict
///     .WithErrorCode("Conflict")                         // 409 Conflict
///     .WithErrorCode(nameof(ConflictRestException));     // 409 Conflict
/// </code>
/// Failures whose error code does not resolve, such as FluentValidation's default <c>"NotEmptyValidator"</c>, count as 400.
/// Only failures with <see cref="Severity.Error"/> decide the status code, unless there are none.
/// If the failures disagree on the status code, the result is 400 Bad Request.
/// <br/>
/// The failures are added to the <c>errors</c> extension, grouped by property name.
/// </remarks>
public sealed class ValidationExceptionMapper : IRestExceptionMapper
{
    public const string ErrorsExtensionKey = "errors";
    public const string MultipleFailuresMessage = "One or more validation errors occurred.";

    public RestException? Map(Exception exception)
    {
        if (exception is not ValidationException validationException)
        {
            return null;
        }

        var failures = validationException.Errors?.Where(failure => failure is not null).ToList() ?? [];

        var extensions = new Dictionary<string, object?>
        {
            { "originalException", exception.GetType().Name }
        };
        if (failures.Count > 0)
        {
            extensions[ErrorsExtensionKey] = failures
                .GroupBy(failure => failure.PropertyName ?? string.Empty)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());
        }

        var message = failures.Count switch
        {
            0 => exception.Message,
            1 => failures[0].ErrorMessage,
            _ => MultipleFailuresMessage
        };

        var statusCode = ResolveStatusCode(failures);

        return RestExceptionFactory.Create(statusCode, message, extensions)
               ?? new BadRequestRestException(message, extensions);
    }

    private static HttpStatusCode ResolveStatusCode(List<ValidationFailure> failures)
    {
        var deciding = failures.Where(failure => failure.Severity == Severity.Error).ToList();
        if (deciding.Count == 0)
        {
            deciding = failures;
        }

        var statusCodes = deciding
            .Select(failure => RestExceptionFactory.TryResolveStatusCode(failure.ErrorCode, out var statusCode)
                ? statusCode
                : HttpStatusCode.BadRequest)
            .Distinct()
            .ToList();

        return statusCodes.Count == 1
            ? statusCodes[0]
            : HttpStatusCode.BadRequest;
    }
}
