using FluentValidation;
using FluentValidation.Results;

namespace RestExceptions.FluentValidation.UnitTests;

public class ValidationExceptionMapperTests
{
    private readonly ValidationExceptionMapper _mapper = new();

    [Test]
    public void Without_Mapper_ValidationException_Is_Internal_Server_Error()
    {
        // Act
        var result = new ValidationException([Failure("409")]).MapToRestException();

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    [Test]
    public void Ignores_Other_Exceptions()
    {
        // Act
        var result = _mapper.Map(new InvalidOperationException());

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    [Arguments("409", HttpStatusCode.Conflict)]
    [Arguments("Conflict", HttpStatusCode.Conflict)]
    [Arguments("conflict", HttpStatusCode.Conflict)]
    [Arguments(nameof(ConflictRestException), HttpStatusCode.Conflict)]
    [Arguments("404", HttpStatusCode.NotFound)]
    [Arguments("UnprocessableEntity", HttpStatusCode.UnprocessableEntity)]
    [Arguments("400", HttpStatusCode.BadRequest)]
    [Arguments("NotEmptyValidator", HttpStatusCode.BadRequest)]
    [Arguments("1", HttpStatusCode.BadRequest)]
    [Arguments("418", HttpStatusCode.BadRequest)]
    [Arguments("999", HttpStatusCode.BadRequest)]
    [Arguments(null, HttpStatusCode.BadRequest)]
    public void Resolves_Status_Code_From_Error_Code(string? errorCode, HttpStatusCode expected)
    {
        // Arrange
        var exception = new ValidationException([Failure(errorCode)]);

        // Act
        var result = _mapper.Map(exception);

        // Assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(expected);
    }

    [Test]
    public void Same_Status_Code_Across_Failures_Is_Kept()
    {
        // Arrange
        var exception = new ValidationException([Failure("409", "Name"), Failure("Conflict", "Email")]);

        // Act
        var result = _mapper.Map(exception);

        // Assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Test]
    public void Mixed_Status_Codes_Are_Bad_Request_With_All_Errors()
    {
        // Arrange
        var exception = new ValidationException(
        [
            Failure("409", "Name", "Name already exists."),
            Failure("NotEmptyValidator", "Email", "Email is required.")
        ]);

        // Act
        var result = _mapper.Map(exception);

        // Assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var errors = Errors(result);
        errors["Name"].ShouldBe(["Name already exists."]);
        errors["Email"].ShouldBe(["Email is required."]);
    }

    [Test]
    public void Only_Error_Severity_Decides_Status_Code()
    {
        // Arrange
        var exception = new ValidationException(
        [
            Failure("409", "Name"),
            Failure("NotEmptyValidator", "Email", severity: Severity.Warning)
        ]);

        // Act
        var result = _mapper.Map(exception);

        // Assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        Errors(result).Keys.ShouldBe(["Name", "Email"], ignoreOrder: true);
    }

    [Test]
    public void Without_Error_Severity_All_Failures_Decide_Status_Code()
    {
        // Arrange
        var exception = new ValidationException([Failure("409", "Name", severity: Severity.Warning)]);

        // Act
        var result = _mapper.Map(exception);

        // Assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Test]
    public void Groups_Errors_By_Property_Name()
    {
        // Arrange
        var exception = new ValidationException(
        [
            Failure(null, "Name", "Name is required."),
            Failure(null, "Name", "Name is too short.")
        ]);

        // Act
        var result = _mapper.Map(exception);

        // Assert
        result.ShouldNotBeNull();
        Errors(result)["Name"].ShouldBe(["Name is required.", "Name is too short."]);
    }

    [Test]
    public void Single_Failure_Message_Is_The_Detail()
    {
        // Arrange
        var exception = new ValidationException([Failure(null, "Name", "Name is required.")]);

        // Act
        var result = _mapper.Map(exception);

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe("Name is required.");
    }

    [Test]
    public void Multiple_Failures_Have_Generic_Detail()
    {
        // Arrange
        var exception = new ValidationException([Failure(null, "Name"), Failure(null, "Email")]);

        // Act
        var result = _mapper.Map(exception);

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe(ValidationExceptionMapper.MultipleFailuresMessage);
    }

    [Test]
    public void Without_Failures_Is_Bad_Request_With_Exception_Message()
    {
        // Arrange
        var exception = new ValidationException("Invalid request.");

        // Act
        var result = _mapper.Map(exception);

        // Assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.Message.ShouldBe("Invalid request.");
        result.Extensions.ShouldNotContainKey(ValidationExceptionMapper.ErrorsExtensionKey);
    }

    [Test]
    public void Keeps_Original_Exception_Extension()
    {
        // Act
        var result = _mapper.Map(new ValidationException([Failure(null)]));

        // Assert
        result.ShouldNotBeNull();
        result.Extensions["originalException"].ShouldBe(nameof(ValidationException));
    }

    private static ValidationFailure Failure(
        string? errorCode,
        string propertyName = "Property",
        string errorMessage = "Invalid.",
        Severity severity = Severity.Error)
    {
        return new ValidationFailure(propertyName, errorMessage)
        {
            ErrorCode = errorCode,
            Severity = severity
        };
    }

    private static Dictionary<string, string[]> Errors(RestException restException)
    {
        return restException.Extensions[ValidationExceptionMapper.ErrorsExtensionKey]
            .ShouldBeOfType<Dictionary<string, string[]>>();
    }
}
