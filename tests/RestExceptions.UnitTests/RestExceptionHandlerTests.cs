using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RestExceptions.UnitTests;

public class RestExceptionHandlerTests
{
    [Fact]
    public async Task Without_Mappers_Uses_Built_In_Mapping()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddRestExceptions()
            .BuildServiceProvider();

        // Act
        var result = await HandleAsync(services, new KeyNotFoundException());

        // Assert
        result.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Registered_Mapper_Takes_Precedence_Over_Built_In_Mapping()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddRestExceptions(options => options.AddMapper<KeyNotFoundToGoneMapper>())
            .BuildServiceProvider();

        // Act
        var result = await HandleAsync(services, new KeyNotFoundException());

        // Assert
        result.ShouldBe(StatusCodes.Status410Gone);
    }

    [Fact]
    public void AddMapper_Registers_Same_Mapper_Once()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddRestExceptions(options => options
                .AddMapper<KeyNotFoundToGoneMapper>()
                .AddMapper<KeyNotFoundToGoneMapper>())
            .BuildServiceProvider();

        // Act
        var result = services.GetServices<IRestExceptionMapper>();

        // Assert
        result.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Constructor_Without_Mappers_Remains_Available()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddRestExceptions()
            .BuildServiceProvider();
        var handler = new RestExceptionHandler(services.GetRequiredService<IProblemDetailsService>());
        var httpContext = new DefaultHttpContext { RequestServices = services };

        // Act
        await handler.TryHandleAsync(httpContext, new KeyNotFoundException());

        // Assert
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Extensions_With_Reserved_Keys_Do_Not_Break_The_Response()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddRestExceptions()
            .BuildServiceProvider();
        var exception = new ConflictRestException(extensions: new Dictionary<string, object?>
        {
            { "method", "custom" },
            { "requestId", "custom" }
        });

        // Act
        var result = await HandleAsync(services, exception);

        // Assert
        result.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Theory]
    [InlineData(typeof(Exception), true)]
    [InlineData(typeof(KeyNotFoundException), false)]
    public async Task Logs_Only_Server_Errors(Type exceptionType, bool expected)
    {
        // Arrange
        var loggerProvider = new CapturingLoggerProvider();
        var services = new ServiceCollection()
            .AddLogging(logging => logging.AddProvider(loggerProvider))
            .AddRestExceptions()
            .BuildServiceProvider();
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        // Act
        await HandleAsync(services, exception);

        // Assert
        loggerProvider.Logged
            .Any(entry => entry.Category == typeof(RestExceptionHandler).FullName && entry.Exception == exception)
            .ShouldBe(expected);
    }

    private static async Task<int> HandleAsync(IServiceProvider services, Exception exception)
    {
        var httpContext = new DefaultHttpContext { RequestServices = services };
        var handler = services.GetServices<IExceptionHandler>().OfType<RestExceptionHandler>().Single();

        await handler.TryHandleAsync(httpContext, exception);

        return httpContext.Response.StatusCode;
    }

    private sealed class KeyNotFoundToGoneMapper : IRestExceptionMapper
    {
        public RestException? Map(Exception exception)
        {
            return exception is KeyNotFoundException
                ? new GoneRestException()
                : null;
        }
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public List<(string Category, Exception? Exception)> Logged { get; } = [];

        public ILogger CreateLogger(string categoryName)
        {
            return new CapturingLogger(categoryName, Logged);
        }

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(string category, List<(string, Exception?)> logged) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                logged.Add((category, exception));
            }
        }
    }
}
