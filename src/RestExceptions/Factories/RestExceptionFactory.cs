using System.Globalization;

namespace RestExceptions;

/// <summary>
/// Creates <see cref="RestException"/>s from an <see cref="HttpStatusCode"/>.
/// </summary>
public static class RestExceptionFactory
{
    private const string TypeNameSuffix = nameof(RestException);

    private static readonly Dictionary<HttpStatusCode, Func<string?, Dictionary<string, object?>?, RestException>>
        Factories = new Func<string?, Dictionary<string, object?>?, RestException>[]
            {
                // 4xx
                (message, extensions) => new BadRequestRestException(message, extensions),
                (message, extensions) => new UnauthorizedRestException(message, extensions),
                (message, extensions) => new PaymentRequiredRestException(message, extensions),
                (message, extensions) => new ForbiddenRestException(message, extensions),
                (message, extensions) => new NotFoundRestException(message, extensions),
                (message, extensions) => new MethodNotAllowedRestException(message, extensions),
                (message, extensions) => new NotAcceptableRestException(message, extensions),
                (message, extensions) => new ProxyAuthenticationRequiredRestException(message, extensions),
                (message, extensions) => new RequestTimeoutRestException(message, extensions),
                (message, extensions) => new ConflictRestException(message, extensions),
                (message, extensions) => new GoneRestException(message, extensions),
                (message, extensions) => new LengthRequiredRestException(message, extensions),
                (message, extensions) => new PreconditionFailedRestException(message, extensions),
                (message, extensions) => new ContentTooLargeRestException(message, extensions),
                (message, extensions) => new UriTooLongRestException(message, extensions),
                (message, extensions) => new UnsupportedMediaTypeRestException(message, extensions),
                (message, extensions) => new RangeNotSatisfiableRestException(message, extensions),
                (message, extensions) => new ExpectationFailedRestException(message, extensions),
                (message, extensions) => new MisdirectedRequestRestException(message, extensions),
                (message, extensions) => new UnprocessableContentRestException(message, extensions),
                (message, extensions) => new LockedRestException(message, extensions),
                (message, extensions) => new FailedDependencyRestException(message, extensions),
                (message, extensions) => new UpgradeRequiredRestException(message, extensions),
                (message, extensions) => new PreconditionRequiredRestException(message, extensions),
                (message, extensions) => new TooManyRequestsRestException(message, extensions),
                (message, extensions) => new RequestHeaderFieldsTooLargeRestException(message, extensions),
                (message, extensions) => new UnavailableForLegalReasonsRestException(message, extensions),
                // 5xx
                (message, extensions) => new InternalServerErrorRestException(message, extensions),
                (message, extensions) => new NotImplementedRestException(message, extensions),
                (message, extensions) => new BadGatewayRestException(message, extensions),
                (message, extensions) => new ServiceUnavailableRestException(message, extensions),
                (message, extensions) => new GatewayTimeoutRestException(message, extensions),
                (message, extensions) => new HttpVersionNotSupportedRestException(message, extensions),
                (message, extensions) => new VariantAlsoNegotiatesRestException(message, extensions),
                (message, extensions) => new InsufficientStorageRestException(message, extensions),
                (message, extensions) => new LoopDetectedRestException(message, extensions),
                (message, extensions) => new NotExtendedRestException(message, extensions),
                (message, extensions) => new NetworkAuthenticationRequiredRestException(message, extensions)
            }
            .ToDictionary(factory => factory(null, null).StatusCode);

    // e.g. "Conflict" for ConflictRestException
    private static readonly Dictionary<string, HttpStatusCode> TypeNames =
        Factories.ToDictionary(
            kvp => kvp.Value(null, null).GetType().Name[..^TypeNameSuffix.Length],
            kvp => kvp.Key,
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The status codes that a <see cref="RestException"/> exists for.
    /// </summary>
    public static IReadOnlyCollection<HttpStatusCode> SupportedStatusCodes => Factories.Keys;

    /// <summary>
    /// Creates the <see cref="RestException"/> that corresponds to the given <paramref name="statusCode"/>.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="message">The exception message. When <c>null</c>, the exception's default message is used.</param>
    /// <param name="extensions">The extensions to add to the resulting problem details.</param>
    /// <returns>The <see cref="RestException"/>, or <c>null</c> if no <see cref="RestException"/> exists for the status code.</returns>
    public static RestException? Create(
        HttpStatusCode statusCode,
        string? message = null,
        Dictionary<string, object?>? extensions = null)
    {
        return Factories.TryGetValue(statusCode, out var factory)
            ? factory(message, extensions)
            : null;
    }

    /// <summary>
    /// Resolves a supported <see cref="HttpStatusCode"/> from its textual representation.
    /// Matching is case-insensitive and accepts:
    /// <list type="bullet">
    /// <item>the numeric code, e.g. <c>"409"</c></item>
    /// <item>the <see cref="RestException"/> type name, with or without its suffix, e.g. <c>"ConflictRestException"</c> or <c>"Conflict"</c></item>
    /// <item>the <see cref="HttpStatusCode"/> member name, e.g. <c>"UnprocessableEntity"</c></item>
    /// </list>
    /// </summary>
    /// <param name="value">The value to resolve.</param>
    /// <param name="statusCode">The resolved status code.</param>
    /// <returns><c>true</c> if <paramref name="value"/> resolves to a status code that a <see cref="RestException"/> exists for.</returns>
    public static bool TryResolveStatusCode(string? value, out HttpStatusCode statusCode)
    {
        statusCode = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
        {
            statusCode = (HttpStatusCode)number;
            return Factories.ContainsKey(statusCode);
        }

        var name = value.EndsWith(TypeNameSuffix, StringComparison.OrdinalIgnoreCase)
            ? value[..^TypeNameSuffix.Length]
            : value;

        if (TypeNames.TryGetValue(name, out statusCode))
        {
            return true;
        }

        // Letters only, so that Enum.TryParse does not accept numbers or comma-separated combinations
        return name.All(char.IsAsciiLetter)
               && Enum.TryParse(name, ignoreCase: true, out statusCode)
               && Factories.ContainsKey(statusCode);
    }
}
