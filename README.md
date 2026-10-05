# RestExceptions

[![CI](https://github.com/stratisd-labs/rest-exceptions-dotnet/workflows/CI/badge.svg)](https://github.com/stratisd-labs/rest-exceptions-dotnet/actions?query=workflow%3ACI)

Extensible Web API middleware that maps all exceptions to standardized [RFC7807](https://www.rfc-editor.org/rfc/rfc7807.html)-compliant HTTP responses.

## NuGet Packages

| RestExceptions Package                                          | NuGet                                                                                                                   |
|-----------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| [RestExceptions](https://www.nuget.org/packages/RestExceptions) | [![NuGet](http://img.shields.io/nuget/vpre/RestExceptions.svg?label=NuGet)](https://www.nuget.org/packages/RestExceptions/) |
| [RestExceptions.FluentValidation](https://www.nuget.org/packages/RestExceptions.FluentValidation) | [![NuGet](http://img.shields.io/nuget/vpre/RestExceptions.FluentValidation.svg?label=NuGet)](https://www.nuget.org/packages/RestExceptions.FluentValidation/) |

## Documentation

You can read the documentation for **RestExceptions** on [GitHub wiki](https://github.com/stratisd-labs/rest-exceptions-dotnet/wiki).

## Example response format (404)

```json
{
  "type": "https://www.rfc-editor.org/rfc/rfc9110.html#name-404-not-found",
  "title": "Not Found",
  "status": 404,
  "detail": "Content not found.",
  "instance": "/users/12",
  "traceId": "00-f3e52009f0966d717f93c9653ed45e26-750dad4218610f7f-00",
  "method": "GET",
  "requestId": "0HNE94J42PNH8:00000001"
}
```

## Mapping additional exceptions

Implement `IRestExceptionMapper` and register it. Mappers run in registration order, before the built-in mapping; the first non-null result wins.

```csharp
public sealed class TimeoutMapper : IRestExceptionMapper
{
    public RestException? Map(Exception exception) =>
        exception is TimeoutException ? new GatewayTimeoutRestException(exception.Message) : null;
}

builder.Services.AddRestExceptions(options => options.AddMapper<TimeoutMapper>());
```

`RestExceptionFactory.Create(HttpStatusCode.Conflict, message)` creates the `RestException` for a status code.

## FluentValidation

Install `RestExceptions.FluentValidation` and register it:

```csharp
builder.Services.AddRestExceptions(options => options.AddFluentValidation());
```

A `ValidationException` then takes its status code from each rule's `ErrorCode`:

```csharp
RuleFor(x => x.Name)
    .Must(BeUnique)
    .WithErrorCode("409");                           // 409 Conflict
    // or .WithErrorCode("Conflict")
    // or .WithErrorCode(nameof(ConflictRestException))
```

- An error code that does not resolve to a status code (e.g. FluentValidation's default `NotEmptyValidator`) means 400 Bad Request.
- Only failures with `Severity.Error` decide the status code, unless there are none.
- If the failures disagree on the status code, the response is 400 Bad Request.
- The failures are returned in an `errors` extension, grouped by property name:

```json
{
  "type": "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
  "title": "Conflict",
  "status": 409,
  "detail": "A user named 'taken' already exists.",
  "originalException": "ValidationException",
  "errors": {
    "Name": [ "A user named 'taken' already exists." ]
  }
}
```

Without `AddFluentValidation()`, a `ValidationException` is returned as 500 Internal Server Error.

## Examples and sample project

A minimal API utilizing **RestExceptions** is included in the project files as an example.

## Disclaimer

This project was generated using [stratisd-labs/dotnet-empty-solution](https://github.com/stratisd-labs/dotnet-empty-solution).
