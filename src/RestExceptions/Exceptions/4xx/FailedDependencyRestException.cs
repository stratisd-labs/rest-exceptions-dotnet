namespace RestExceptions;

/// <summary>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Status/424"/>
/// </summary>
public class FailedDependencyRestException(
    string? message = null,
    Dictionary<string, object?>? extensions = null)
    : RestException(message ?? DefaultMessage, extensions), IRestException
{
    public static string DefaultMessage => "The requested action depended on another action, and that action failed.";

    public override string Title => "Failed Dependency";
    public override HttpStatusCode StatusCode => HttpStatusCode.FailedDependency;
}
