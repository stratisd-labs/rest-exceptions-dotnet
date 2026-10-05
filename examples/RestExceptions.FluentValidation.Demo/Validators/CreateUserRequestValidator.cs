using FluentValidation;

namespace RestExceptions.FluentValidation.Demo.Validators;

public sealed record CreateUserRequest(string Name, string Email);

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        // No error code: FluentValidation's defaults (e.g. "NotEmptyValidator") map to 400 Bad Request
        RuleFor(request => request.Name)
            .NotEmpty();
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();

        // Numeric status code: 409 Conflict
        RuleFor(request => request.Name)
            .Must(name => name != "taken")
            .WithErrorCode("409")
            .WithMessage(request => $"A user named '{request.Name}' already exists.");

        // Status name: 422 Unprocessable Content
        RuleFor(request => request.Name)
            .Must(name => name != "admin")
            .WithErrorCode("UnprocessableContent")
            .WithMessage("The name 'admin' is reserved.");

        // RestException type name: 403 Forbidden
        RuleFor(request => request.Email)
            .Must(email => !email.EndsWith("@blocked.example", StringComparison.OrdinalIgnoreCase))
            .WithErrorCode(nameof(ForbiddenRestException))
            .WithMessage("Emails from 'blocked.example' are not allowed.");
    }
}
