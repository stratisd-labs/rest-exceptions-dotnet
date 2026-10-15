namespace RestExceptions;

[Serializable]
public abstract class RestException(string message, Dictionary<string, object?>? extensions) : Exception(message)
{
    public abstract string Title { get; }
    public abstract HttpStatusCode StatusCode { get; }

    public Dictionary<string, object?> Extensions { get; } = extensions ?? [];

    [Obsolete("Builds RFC 9110 anchors that don't exist for every status code. Use ProblemTypes.Get(StatusCode) instead.")]
    public string TypeSuffix =>$"{(int)StatusCode}-{Title.ToKebabCase()}";
}
