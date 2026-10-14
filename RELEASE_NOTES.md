# Release Notes

All notable changes to **RestExceptions** and **RestExceptions.FluentValidation**. Newest releases first.

## Unreleased

### ✨ New
- `ProblemTypes.Get(statusCode)` returns the `type` URI that the default builder uses, for custom `IRestExceptionProblemDetailsBuilder`s.

### 🐛 Fixes
- A request whose `Accept` header didn't allow JSON (e.g. `text/html`) got a 500 error instead of the problem details response. The response is now always written as `application/problem+json`, as [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457.html) allows.
- Fixed broken `type` links for 13 status codes:
  - 423, 424, 428, 429, 431, 451, 506, 507, 508, 510 and 511 now link to the RFC that defines them, instead of RFC 9110, which doesn't.
  - 407 and 505 linked to RFC 9110 anchors that don't exist.
  - All `type` links now point to section numbers, e.g. `rfc9110.html#section-15.5.5` instead of `rfc9110.html#name-404-not-found`.

## 10.1.0 - 2026-10-13

This release adds a way to map your own exception types, a new **RestExceptions.FluentValidation** package, and logging for server errors on .NET 10.

### ✨ New

#### New package: RestExceptions.FluentValidation
Maps FluentValidation's `ValidationException` to a problem details response. The status code comes from each rule's `ErrorCode`.

```csharp
builder.Services.AddRestExceptions(options => options.AddFluentValidation());

RuleFor(x => x.Name)
    .Must(BeUnique)
    .WithErrorCode("409");   // or "Conflict", or nameof(ConflictRestException)
```

- An error code that doesn't match a status code (e.g. FluentValidation's default `NotEmptyValidator`) gives 400 Bad Request.
- Only failures with `Severity.Error` decide the status code, unless there are none.
- If the failures point to different status codes, the response is 400 Bad Request.
- All failures are returned in an `errors` extension, grouped by property name.
- Requires FluentValidation 11.0.0 or later.

#### `IRestExceptionMapper`
Map any exception type to a `RestException`. Mappers run in the order they're registered, before the built-in mapping, and the first one that returns a result wins.

```csharp
public sealed class TimeoutMapper : IRestExceptionMapper
{
    public RestException? Map(Exception exception) =>
        exception is TimeoutException ? new GatewayTimeoutRestException(exception.Message) : null;
}

builder.Services.AddRestExceptions(options => options.AddMapper<TimeoutMapper>());
```

#### `RestExceptionFactory`
- `RestExceptionFactory.Create(HttpStatusCode.Conflict, message)` creates the `RestException` for a status code.
- `RestExceptionFactory.TryResolveStatusCode("Conflict", out var statusCode)` turns `"409"`, `"Conflict"`, `"ConflictRestException"` or `"UnprocessableEntity"` into a status code.

#### Server errors are logged again
Since .NET 10, ASP.NET Core no longer logs exceptions handled by an `IExceptionHandler`, so server errors weren't logged at all. `RestExceptionHandler` now logs every exception that maps to a 5xx status code as an error. 4xx responses aren't logged.

#### IntelliSense documentation
Both packages now include their XML documentation.

### 🐛 Fixes
- A `RestException` whose extensions contained a `method` or `requestId` key caused an empty 500 response. Your values are now kept.
- Fixed a typo in `FailedDependencyRestException.DefaultMessage` ("Te requested action…" → "The requested action…").

### ⚠️ Breaking changes
- **`InvalidOperationException` now maps to 500 Internal Server Error** instead of 405 Method Not Allowed. 405 means the HTTP method isn't supported, which these exceptions don't indicate. This also affects types that inherit from it, such as `ObjectDisposedException`. To keep a different status code, register an `IRestExceptionMapper`.
- **`StringExtensions.ToKebabCase` is now internal.** It is no longer an extension method on every `string` in projects that use the `RestExceptions` namespace.

### 🔧 Internal
- The tests now run on TUnit instead of xUnit.

### 📦 Packages
| Package | Version |
|---|---|
| [RestExceptions](https://www.nuget.org/packages/RestExceptions/10.1.0) | 10.1.0 |
| [RestExceptions.FluentValidation](https://www.nuget.org/packages/RestExceptions.FluentValidation/10.1.0) | 10.1.0 (new) |

## 10.0.0 - 2025-11-16
- Upgraded to .NET 10 and C# 14.

## 9.0.11 - 2025-11-16
- Updated dependencies to the latest versions.

## 9.0.9 - 2025-09-17
- Updated dependencies to the latest versions.
- Applied code style.

## 9.0.8 - 2025-08-06
- Better automatic mapping of .NET's built-in exceptions.
- Updated dependencies.

## 9.0.7.4 - 2025-07-26
- Made the default `IRestExceptionProblemDetailsBuilder` implementation public.

## 9.0.7.3 - 2025-07-24
- Added support for all common HTTP 4xx and 5xx status codes.
- Added a global exception handler.
- Added an easily overridable `ProblemDetails` builder.
- Wrote complete documentation.
- Added support for more .NET versions.

## 9.0.7.2 - 2025-07-21
- Fixed the `instance` property.
- Added the `method` property.

## 9.0.7.1 - 2025-07-20
- Fixed the response `type` reference to point to [RFC 9110 HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110.html).

## 9.0.7 - 2025-07-20
- Initial release, with the default middleware implementation.
