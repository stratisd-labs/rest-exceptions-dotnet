using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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
}
