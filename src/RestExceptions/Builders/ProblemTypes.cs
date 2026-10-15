namespace RestExceptions;

/// <summary>
/// Resolves the <see href="https://www.rfc-editor.org/rfc/rfc9457.html#name-type">problem type URI</see> of an HTTP status code,
/// i.e. a link to the section of the RFC that defines the status code.
/// </summary>
public static class ProblemTypes
{
    /// <summary>
    /// The problem type that has no meaning beyond the HTTP status code,
    /// as defined in <see href="https://www.rfc-editor.org/rfc/rfc9457.html#name-aboutblank">RFC 9457</see>.
    /// </summary>
    public const string AboutBlank = "about:blank";

    private const string Rfc9110 = "https://www.rfc-editor.org/rfc/rfc9110.html";

    // Section anchors, since the "#name-..." anchors are truncated for long titles, e.g. "#name-407-proxy-authentication-re"
    private static readonly Dictionary<HttpStatusCode, string> Uris = new()
    {
        // 4xx
        [HttpStatusCode.BadRequest] = $"{Rfc9110}#section-15.5.1",
        [HttpStatusCode.Unauthorized] = $"{Rfc9110}#section-15.5.2",
        [HttpStatusCode.PaymentRequired] = $"{Rfc9110}#section-15.5.3",
        [HttpStatusCode.Forbidden] = $"{Rfc9110}#section-15.5.4",
        [HttpStatusCode.NotFound] = $"{Rfc9110}#section-15.5.5",
        [HttpStatusCode.MethodNotAllowed] = $"{Rfc9110}#section-15.5.6",
        [HttpStatusCode.NotAcceptable] = $"{Rfc9110}#section-15.5.7",
        [HttpStatusCode.ProxyAuthenticationRequired] = $"{Rfc9110}#section-15.5.8",
        [HttpStatusCode.RequestTimeout] = $"{Rfc9110}#section-15.5.9",
        [HttpStatusCode.Conflict] = $"{Rfc9110}#section-15.5.10",
        [HttpStatusCode.Gone] = $"{Rfc9110}#section-15.5.11",
        [HttpStatusCode.LengthRequired] = $"{Rfc9110}#section-15.5.12",
        [HttpStatusCode.PreconditionFailed] = $"{Rfc9110}#section-15.5.13",
        [HttpStatusCode.RequestEntityTooLarge] = $"{Rfc9110}#section-15.5.14",
        [HttpStatusCode.RequestUriTooLong] = $"{Rfc9110}#section-15.5.15",
        [HttpStatusCode.UnsupportedMediaType] = $"{Rfc9110}#section-15.5.16",
        [HttpStatusCode.RequestedRangeNotSatisfiable] = $"{Rfc9110}#section-15.5.17",
        [HttpStatusCode.ExpectationFailed] = $"{Rfc9110}#section-15.5.18",
        [HttpStatusCode.MisdirectedRequest] = $"{Rfc9110}#section-15.5.20",
        [HttpStatusCode.UnprocessableContent] = $"{Rfc9110}#section-15.5.21",
        [HttpStatusCode.Locked] = "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.3",
        [HttpStatusCode.FailedDependency] = "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.4",
        [HttpStatusCode.UpgradeRequired] = $"{Rfc9110}#section-15.5.22",
        [HttpStatusCode.PreconditionRequired] = "https://www.rfc-editor.org/rfc/rfc6585.html#section-3",
        [HttpStatusCode.TooManyRequests] = "https://www.rfc-editor.org/rfc/rfc6585.html#section-4",
        [HttpStatusCode.RequestHeaderFieldsTooLarge] = "https://www.rfc-editor.org/rfc/rfc6585.html#section-5",
        [HttpStatusCode.UnavailableForLegalReasons] = "https://www.rfc-editor.org/rfc/rfc7725.html#section-3",
        // 5xx
        [HttpStatusCode.InternalServerError] = $"{Rfc9110}#section-15.6.1",
        [HttpStatusCode.NotImplemented] = $"{Rfc9110}#section-15.6.2",
        [HttpStatusCode.BadGateway] = $"{Rfc9110}#section-15.6.3",
        [HttpStatusCode.ServiceUnavailable] = $"{Rfc9110}#section-15.6.4",
        [HttpStatusCode.GatewayTimeout] = $"{Rfc9110}#section-15.6.5",
        [HttpStatusCode.HttpVersionNotSupported] = $"{Rfc9110}#section-15.6.6",
        [HttpStatusCode.VariantAlsoNegotiates] = "https://www.rfc-editor.org/rfc/rfc2295.html#section-8.1",
        [HttpStatusCode.InsufficientStorage] = "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.5",
        [HttpStatusCode.LoopDetected] = "https://www.rfc-editor.org/rfc/rfc5842.html#section-7.2",
        [HttpStatusCode.NotExtended] = "https://www.rfc-editor.org/rfc/rfc2774.html#section-7",
        [HttpStatusCode.NetworkAuthenticationRequired] = "https://www.rfc-editor.org/rfc/rfc6585.html#section-6"
    };

    /// <summary>
    /// Gets the problem type URI of the given <paramref name="statusCode"/>.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The URI of the RFC section that defines the status code, or <see cref="AboutBlank"/> for any other status code.</returns>
    /// <remarks>
    /// With <see cref="AboutBlank"/>, RFC 9457 expects the title to be the HTTP status phrase.
    /// For a custom <see cref="RestException"/> with its own title, override the <see cref="IRestExceptionProblemDetailsBuilder"/> to supply its own type.
    /// </remarks>
    /// <example>
    /// <c>HttpStatusCode.NotFound</c> becomes <c>"https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.5"</c>.
    /// </example>
    public static string Get(HttpStatusCode statusCode)
    {
        return Uris.GetValueOrDefault(statusCode, AboutBlank);
    }
}
