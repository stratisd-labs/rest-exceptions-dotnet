namespace RestExceptions.UnitTests;

public class RestExceptionFactoryTests
{
    [Fact]
    public void Supports_Every_RestException_Type()
    {
        // Arrange
        var restExceptionTypes = typeof(RestException).Assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false } && type.IsSubclassOf(typeof(RestException)))
            .ToList();

        // Act
        var result = RestExceptionFactory.SupportedStatusCodes;

        // Assert
        result.Count.ShouldBe(restExceptionTypes.Count);
    }

    [Fact]
    public void Create_Returns_RestException_With_Requested_Status_Code()
    {
        foreach (var statusCode in RestExceptionFactory.SupportedStatusCodes)
        {
            // Act
            var result = RestExceptionFactory.Create(statusCode);

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(statusCode);
        }
    }

    [Fact]
    public void Create_Passes_Message_And_Extensions()
    {
        // Arrange
        var extensions = new Dictionary<string, object?> { { "key", "value" } };

        // Act
        var result = RestExceptionFactory.Create(HttpStatusCode.Conflict, "message", extensions);

        // Assert
        result.ShouldBeOfType<ConflictRestException>();
        result.Message.ShouldBe("message");
        result.Extensions.ShouldBe(extensions);
    }

    [Fact]
    public void Create_Uses_Default_Message_When_Message_Is_Null()
    {
        // Act
        var result = RestExceptionFactory.Create(HttpStatusCode.Conflict);

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe(ConflictRestException.DefaultMessage);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData((HttpStatusCode)418)]
    [InlineData((HttpStatusCode)999)]
    public void Create_Returns_Null_For_Unsupported_Status_Code(HttpStatusCode statusCode)
    {
        // Act
        var result = RestExceptionFactory.Create(statusCode);

        // Assert
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("409", HttpStatusCode.Conflict)]
    [InlineData("Conflict", HttpStatusCode.Conflict)]
    [InlineData("conflict", HttpStatusCode.Conflict)]
    [InlineData("ConflictRestException", HttpStatusCode.Conflict)]
    [InlineData("conflictrestexception", HttpStatusCode.Conflict)]
    [InlineData("BadRequest", HttpStatusCode.BadRequest)]
    [InlineData("BadRequestRestException", HttpStatusCode.BadRequest)]
    [InlineData("ContentTooLarge", HttpStatusCode.RequestEntityTooLarge)]
    [InlineData("RequestEntityTooLarge", HttpStatusCode.RequestEntityTooLarge)]
    [InlineData("UnprocessableEntity", HttpStatusCode.UnprocessableEntity)]
    [InlineData("UnprocessableContent", HttpStatusCode.UnprocessableEntity)]
    [InlineData("500", HttpStatusCode.InternalServerError)]
    public void TryResolveStatusCode_Resolves_Supported_Values(string value, HttpStatusCode expected)
    {
        // Act
        var result = RestExceptionFactory.TryResolveStatusCode(value, out var statusCode);

        // Assert
        result.ShouldBeTrue();
        statusCode.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1")]
    [InlineData("200")]
    [InlineData("418")]
    [InlineData("999")]
    [InlineData("-409")]
    [InlineData("+409")]
    [InlineData(" 409")]
    [InlineData("OK")]
    [InlineData("NotEmptyValidator")]
    [InlineData("Conflict,Gone")]
    [InlineData("RestException")]
    public void TryResolveStatusCode_Rejects_Unsupported_Values(string? value)
    {
        // Act
        var result = RestExceptionFactory.TryResolveStatusCode(value, out _);

        // Assert
        result.ShouldBeFalse();
    }
}
