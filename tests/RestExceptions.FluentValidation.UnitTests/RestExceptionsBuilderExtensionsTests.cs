using Microsoft.Extensions.DependencyInjection;

namespace RestExceptions.FluentValidation.UnitTests;

public class RestExceptionsBuilderExtensionsTests
{
    [Fact]
    public void AddFluentValidation_Registers_ValidationExceptionMapper()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddRestExceptions(options => options.AddFluentValidation())
            .BuildServiceProvider();

        // Act
        var result = services.GetServices<IRestExceptionMapper>();

        // Assert
        result.ShouldHaveSingleItem().ShouldBeOfType<ValidationExceptionMapper>();
    }
}
