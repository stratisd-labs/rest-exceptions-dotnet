using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RestExceptions;
using RestExceptions.FluentValidation;
using RestExceptions.FluentValidation.Demo.Validators;

var builder = WebApplication.CreateBuilder(args);

// AddFluentValidation() maps FluentValidation's ValidationException, using each rule's ErrorCode as the status code.
// Without it, a ValidationException is returned as 500 Internal Server Error.
builder.Services.AddRestExceptions(options => options.AddFluentValidation());

var app = builder.Build();

app.MapPost("users", ([FromBody] CreateUserRequest request) =>
{
    new CreateUserRequestValidator().ValidateAndThrow(request);
    return Results.Created();
});

//! Important
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.Run();

// Needed so WebApplicationFactory<T> in tests can find the entry point
public partial class Program;
