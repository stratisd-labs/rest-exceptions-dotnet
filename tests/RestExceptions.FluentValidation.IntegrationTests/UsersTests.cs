using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RestExceptions.FluentValidation.IntegrationTests;

public class UsersTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateHttpsClient();

    [Fact]
    public async Task Valid_Request_Is_Created()
    {
        // Act
        var response = await _client.PostAsJsonAsync("users", new { name = "john", email = "john@example.com" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData("", "john@example.com", HttpStatusCode.BadRequest, "Bad Request")]
    [InlineData("taken", "john@example.com", HttpStatusCode.Conflict, "Conflict")]
    [InlineData("admin", "john@example.com", HttpStatusCode.UnprocessableEntity, "Unprocessable Content")]
    [InlineData("john", "john@blocked.example", HttpStatusCode.Forbidden, "Forbidden")]
    public async Task Error_Code_Sets_Status_Code(
        string name,
        string email,
        HttpStatusCode expectedStatusCode,
        string expectedTitle)
    {
        // Act
        var (response, problemDetails) = await PostAsync(_client, name, email);

        // Assert
        response.StatusCode.ShouldBe(expectedStatusCode);
        problemDetails.Status.ShouldBe((int)expectedStatusCode);
        problemDetails.Title.ShouldBe(expectedTitle);
    }

    [Fact]
    public async Task Single_Failure_Is_The_Detail_And_In_Errors()
    {
        // Act
        var (_, problemDetails) = await PostAsync(_client, "taken", "john@example.com");

        // Assert
        problemDetails.Detail.ShouldBe("A user named 'taken' already exists.");
        Errors(problemDetails)["Name"].ShouldBe(["A user named 'taken' already exists."]);
    }

    [Fact]
    public async Task Disagreeing_Error_Codes_Are_Bad_Request_With_All_Errors()
    {
        // Act
        var (response, problemDetails) = await PostAsync(_client, "taken", "john@blocked.example");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problemDetails.Detail.ShouldBe(ValidationExceptionMapper.MultipleFailuresMessage);
        var errors = Errors(problemDetails);
        errors["Name"].ShouldBe(["A user named 'taken' already exists."]);
        errors["Email"].ShouldBe(["Emails from 'blocked.example' are not allowed."]);
    }

    [Fact]
    public async Task Failures_Of_The_Same_Property_Are_Grouped()
    {
        // Act
        var (response, problemDetails) = await PostAsync(_client, "john", "");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        Errors(problemDetails)["Email"].Length.ShouldBe(2);
    }

    [Fact]
    public async Task Without_FluentValidation_Mapper_Is_Internal_Server_Error()
    {
        // Arrange
        await using var withoutMapper = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.RemoveAll<IRestExceptionMapper>()));
        var client = withoutMapper.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        var (response, problemDetails) = await PostAsync(client, "taken", "john@example.com");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        problemDetails.Extensions.ShouldNotContainKey(ValidationExceptionMapper.ErrorsExtensionKey);
    }

    private static async Task<(HttpResponseMessage Response, ProblemDetails ProblemDetails)> PostAsync(
        HttpClient client,
        string name,
        string email)
    {
        var response = await client.PostAsJsonAsync("users", new { name, email });
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        return (response, problemDetails.ShouldNotBeNull());
    }

    private static Dictionary<string, string[]> Errors(ProblemDetails problemDetails)
    {
        var errors = problemDetails.Extensions[ValidationExceptionMapper.ErrorsExtensionKey].ShouldBeOfType<JsonElement>();
        return errors.Deserialize<Dictionary<string, string[]>>()!;
    }
}
