using Microsoft.AspNetCore.Mvc;
using RestExceptions;
// using RestExceptions.Demo.Builders;

var builder = WebApplication.CreateBuilder(args);

// Uncomment the following line to use a custom implementation of IRestExceptionProblemDetailsBuilder.
// builder.Services.AddSingleton<IRestExceptionProblemDetailsBuilder, CustomExampleRestExceptionProblemDetailsBuilder>();

builder.Services.AddRestExceptions();

var app = builder.Build();

app.MapGet("error/{statusCode:int}", ([FromRoute] int statusCode) =>
{
    throw statusCode switch
    {
        // 4xx
        400 => new BadRequestRestException(),
        401 => new UnauthorizedRestException(),
        402 => new PaymentRequiredRestException(),
        403 => new ForbiddenRestException(),
        404 => new NotFoundRestException(),
        405 => new MethodNotAllowedRestException(),
        406 => new NotAcceptableRestException(),
        407 => new ProxyAuthenticationRequiredRestException(),
        408 => new RequestTimeoutRestException(),
        409 => new ConflictRestException(),
        410 => new GoneRestException(),
        411 => new LengthRequiredRestException(),
        412 => new PreconditionFailedRestException(),
        413 => new ContentTooLargeRestException(),
        414 => new UriTooLongRestException(),
        415 => new UnsupportedMediaTypeRestException(),
        416 => new RangeNotSatisfiableRestException(),
        417 => new ExpectationFailedRestException(),
        421 => new MisdirectedRequestRestException(),
        422 => new UnprocessableContentRestException(),
        423 => new LockedRestException(),
        424 => new FailedDependencyRestException(),
        426 => new UpgradeRequiredRestException(),
        428 => new PreconditionRequiredRestException(),
        429 => new TooManyRequestsRestException(),
        431 => new RequestHeaderFieldsTooLargeRestException(),
        451 => new UnavailableForLegalReasonsRestException(),
        // 5xx
        500 => new InternalServerErrorRestException(),
        501 => new NotImplementedRestException(),
        502 => new BadGatewayRestException(),
        503 => new ServiceUnavailableRestException(),
        504 => new GatewayTimeoutRestException(),
        505 => new HttpVersionNotSupportedRestException(),
        506 => new VariantAlsoNegotiatesRestException(),
        507 => new InsufficientStorageRestException(),
        508 => new LoopDetectedRestException(),
        510 => new NotExtendedRestException(),
        511 => new NetworkAuthenticationRequiredRestException(),
        _ => new Exception()
    };
});

//! Important
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.Run();
