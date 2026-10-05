using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace RestExceptions;

public static class ExceptionExtensions
{
    /// <summary>
    /// Maps a generic <see cref="Exception"/> to a specific <see cref="RestException"/>,
    /// giving the provided <paramref name="mappers"/> precedence over the built-in mapping.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <param name="mappers">The mappers to try, in order. The first non-null result wins.</param>
    /// <returns>A <see cref="RestException"/> that corresponds to the provided exception.</returns>
    public static RestException MapToRestException(this Exception exception, IEnumerable<IRestExceptionMapper> mappers)
    {
        if (exception is RestException restException)
        {
            return restException;
        }

        foreach (var mapper in mappers)
        {
            var mapped = mapper.Map(exception);
            if (mapped is not null)
            {
                return mapped;
            }
        }

        return exception.MapToRestException();
    }

    /// <summary>
    /// Maps a generic <see cref="Exception"/> to a specific <see cref="RestException"/>.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>A <see cref="RestException"/> that corresponds to the type of the provided exception.</returns>
    public static RestException MapToRestException(this Exception exception)
    {
        if (exception is RestException restException)
        {
            return restException;
        }

        var extensions = new Dictionary<string, object?>
        {
            { "originalException", exception.GetType().Name }
        };
        return exception switch
        {
            // 404 - Not Found
            CultureNotFoundException
                or DirectoryNotFoundException
                or DllNotFoundException
                or DriveNotFoundException
                or EntryPointNotFoundException
                or EventLogNotFoundException
                or FileNotFoundException
                or KeyNotFoundException
                or TimeZoneNotFoundException
                or VersionNotFoundException => new NotFoundRestException(exception.Message, extensions),
            // 400 - Bad Request
            ArgumentException
                or ArgumentNullException
                or ArgumentOutOfRangeException
                or BadHttpRequestException
                or ConstraintException => new BadRequestRestException(exception.Message, extensions),
            // 403 - Forbidden
            AccessViolationException
                or UnauthorizedAccessException => new ForbiddenRestException(exception.Message, extensions),
            // 405 - Method Not Allowed
            InvalidOperationException => new MethodNotAllowedRestException(exception.Message, extensions),
            // 501 - Not Implemented
            NotImplementedException
                or TypeLoadException => new NotImplementedRestException(exception.Message, extensions),
            // 500 - Internal Server Error (Default)
            _ => new InternalServerErrorRestException(exception.Message, extensions)
        };
    }
}
