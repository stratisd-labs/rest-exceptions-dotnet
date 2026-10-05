namespace RestExceptions.UnitTests;

internal sealed class StubMapper(Func<Exception, RestException?> map) : IRestExceptionMapper
{
    public int Calls { get; private set; }

    public RestException? Map(Exception exception)
    {
        Calls++;
        return map(exception);
    }
}
