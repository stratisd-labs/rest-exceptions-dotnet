using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace RestExceptions.IntegrationTests.Services;

public class FluentValidationTests(ApiFactory factory) : ApiTests(factory), IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Default_Error_Code_Is_Bad_Request_With_Errors()
    {
        // Act
        var response = await Client.PostAsJsonAsync("users", new { name = "" });
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(400);
        problemDetails.Title.ShouldBe("Bad Request");
        var errors = Errors(problemDetails);
        errors["Name"].ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Error_Code_Sets_Status_Code()
    {
        // Act
        var response = await Client.PostAsJsonAsync("users", new { name = "taken" });
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problemDetails.ShouldNotBeNull();
        problemDetails.Title.ShouldBe("Conflict");
        problemDetails.Detail.ShouldBe("A user named 'taken' already exists.");
        Errors(problemDetails)["Name"].ShouldBe(["A user named 'taken' already exists."]);
    }

    private static Dictionary<string, string[]> Errors(ProblemDetails problemDetails)
    {
        var errors = problemDetails.Extensions["errors"].ShouldBeOfType<JsonElement>();
        return errors.Deserialize<Dictionary<string, string[]>>()!;
    }
}
