using Microsoft.AspNetCore.Http;

namespace RestExceptions.UnitTests;

public class ProblemTypesTests
{
    [Test]
    public void Every_Supported_Status_Code_Has_Its_Own_Problem_Type()
    {
        // Act
        var result = RestExceptionFactory.SupportedStatusCodes.Select(ProblemTypes.Get).ToList();

        // Assert
        result.ShouldNotContain(ProblemTypes.AboutBlank);
        result.ShouldBeUnique();
    }

    [Test]
    [Arguments(HttpStatusCode.NotFound, "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.5")]
    [Arguments(HttpStatusCode.ProxyAuthenticationRequired, "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.8")]
    [Arguments(HttpStatusCode.HttpVersionNotSupported, "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.6")]
    [Arguments(HttpStatusCode.Locked, "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.3")]
    [Arguments(HttpStatusCode.TooManyRequests, "https://www.rfc-editor.org/rfc/rfc6585.html#section-4")]
    [Arguments(HttpStatusCode.UnavailableForLegalReasons, "https://www.rfc-editor.org/rfc/rfc7725.html#section-3")]
    [Arguments(HttpStatusCode.VariantAlsoNegotiates, "https://www.rfc-editor.org/rfc/rfc2295.html#section-8.1")]
    public void Status_Code_Links_To_The_Rfc_That_Defines_It(HttpStatusCode statusCode, string expected)
    {
        // Act
        var result = ProblemTypes.Get(statusCode);

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void Unknown_Status_Code_Is_About_Blank()
    {
        // Act
        var result = ProblemTypes.Get((HttpStatusCode)499);

        // Assert
        result.ShouldBe("about:blank");
    }

    [Test]
    public void Default_Builder_Uses_The_Problem_Type_Of_The_Status_Code()
    {
        // Arrange
        var builder = new DefaultRestExceptionProblemDetailsBuilder();

        // Act
        var result = builder.Build(new DefaultHttpContext(), new LockedRestException());

        // Assert
        result.Type.ShouldBe(ProblemTypes.Get(HttpStatusCode.Locked));
    }
}
