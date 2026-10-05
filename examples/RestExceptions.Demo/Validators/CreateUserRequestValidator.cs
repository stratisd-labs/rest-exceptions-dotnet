using FluentValidation;

namespace RestExceptions.Demo.Validators;

public sealed record CreateUserRequest(string Name);

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        // No error code: FluentValidation's default ("NotEmptyValidator") maps to 400 Bad Request
        RuleFor(request => request.Name)
            .NotEmpty();

        // Also valid: .WithErrorCode("Conflict") or .WithErrorCode(nameof(ConflictRestException))
        RuleFor(request => request.Name)
            .Must(name => name != "taken")
            .WithErrorCode("409")
            .WithMessage(request => $"A user named '{request.Name}' already exists.");
    }
}
