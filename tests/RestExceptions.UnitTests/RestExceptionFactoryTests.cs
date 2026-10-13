namespace RestExceptions.UnitTests;

public class RestExceptionFactoryTests
{
    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
    public void Create_Uses_Default_Message_When_Message_Is_Null()
    {
        // Act
        var result = RestExceptionFactory.Create(HttpStatusCode.Conflict);

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe(ConflictRestException.DefaultMessage);
    }

    [Test]
    [Arguments(HttpStatusCode.OK)]
    [Arguments((HttpStatusCode)418)]
    [Arguments((HttpStatusCode)999)]
    public void Create_Returns_Null_For_Unsupported_Status_Code(HttpStatusCode statusCode)
    {
        // Act
        var result = RestExceptionFactory.Create(statusCode);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    [Arguments("409", HttpStatusCode.Conflict)]
    [Arguments("Conflict", HttpStatusCode.Conflict)]
    [Arguments("conflict", HttpStatusCode.Conflict)]
    [Arguments("ConflictRestException", HttpStatusCode.Conflict)]
    [Arguments("conflictrestexception", HttpStatusCode.Conflict)]
    [Arguments("BadRequest", HttpStatusCode.BadRequest)]
    [Arguments("BadRequestRestException", HttpStatusCode.BadRequest)]
    [Arguments("ContentTooLarge", HttpStatusCode.RequestEntityTooLarge)]
    [Arguments("RequestEntityTooLarge", HttpStatusCode.RequestEntityTooLarge)]
    [Arguments("UnprocessableEntity", HttpStatusCode.UnprocessableEntity)]
    [Arguments("UnprocessableContent", HttpStatusCode.UnprocessableEntity)]
    [Arguments("500", HttpStatusCode.InternalServerError)]
    public void TryResolveStatusCode_Resolves_Supported_Values(string value, HttpStatusCode expected)
    {
        // Act
        var result = RestExceptionFactory.TryResolveStatusCode(value, out var statusCode);

        // Assert
        result.ShouldBeTrue();
        statusCode.ShouldBe(expected);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(" ")]
    [Arguments("1")]
    [Arguments("200")]
    [Arguments("418")]
    [Arguments("999")]
    [Arguments("-409")]
    [Arguments("+409")]
    [Arguments(" 409")]
    [Arguments("OK")]
    [Arguments("NotEmptyValidator")]
    [Arguments("Conflict,Gone")]
    [Arguments("RestException")]
    public void TryResolveStatusCode_Rejects_Unsupported_Values(string? value)
    {
        // Act
        var result = RestExceptionFactory.TryResolveStatusCode(value, out _);

        // Assert
        result.ShouldBeFalse();
    }
}
