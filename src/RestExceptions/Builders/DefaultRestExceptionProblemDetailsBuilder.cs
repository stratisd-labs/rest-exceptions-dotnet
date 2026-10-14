using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RestExceptions;

public sealed class DefaultRestExceptionProblemDetailsBuilder : IRestExceptionProblemDetailsBuilder
{
    public ProblemDetails Build(HttpContext httpContext, RestException restException)
    {
        var problemDetails = new ProblemDetails
        {
            Status = (int)restException.StatusCode,
            Title = restException.Title,
            Detail = restException.Message,
            Type = ProblemTypes.Get(restException.StatusCode)
        };

        return problemDetails;
    }
}
