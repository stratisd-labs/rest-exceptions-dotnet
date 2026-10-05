using Microsoft.AspNetCore.Mvc.Testing;

namespace RestExceptions.FluentValidation.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>
{
    public HttpClient CreateHttpsClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }
}
