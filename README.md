# RestExceptions

[![CI](https://github.com/stratisd-labs/rest-exceptions-dotnet/workflows/CI/badge.svg)](https://github.com/stratisd-labs/rest-exceptions-dotnet/actions?query=workflow%3ACI)

Extensible Web API middleware that maps all exceptions to standardized [RFC7807](https://www.rfc-editor.org/rfc/rfc7807.html)-compliant HTTP responses.

## NuGet Packages

| RestExceptions Package                                          | NuGet                                                                                                                   |
|-----------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| [RestExceptions](https://www.nuget.org/packages/RestExceptions) | [![NuGet](http://img.shields.io/nuget/vpre/RestExceptions.svg?label=NuGet)](https://www.nuget.org/packages/RestExceptions/) |

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

## Examples and sample project

A minimal API utilizing **RestExceptions** is included in the project files as an example.

## Disclaimer

This project was generated using [stratisd-labs/dotnet-empty-solution](https://github.com/stratisd-labs/dotnet-empty-solution).
