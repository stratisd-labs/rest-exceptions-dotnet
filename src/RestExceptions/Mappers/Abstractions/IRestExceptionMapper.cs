namespace RestExceptions;

/// <summary>
/// Maps an <see cref="Exception"/> to a <see cref="RestException"/>.
///
/// Mappers run, in registration order, before the built-in mapping of <see cref="ExceptionExtensions.MapToRestException"/>.
/// The first mapper to return a non-null result wins; if none does, the built-in mapping is used.
/// <see cref="RestException"/>s thrown directly are never passed to a mapper.
/// </summary>
/// <code>
/// // In your Startup or Program class
/// builder.Services.AddRestExceptions(options => options.AddMapper&lt;CustomRestExceptionMapper&gt;());
/// </code>
public interface IRestExceptionMapper
{
    /// <summary>
    /// Maps the given <paramref name="exception"/> to a <see cref="RestException"/>.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>The mapped <see cref="RestException"/>, or <c>null</c> if this mapper does not handle the exception.</returns>
    RestException? Map(Exception exception);
}
