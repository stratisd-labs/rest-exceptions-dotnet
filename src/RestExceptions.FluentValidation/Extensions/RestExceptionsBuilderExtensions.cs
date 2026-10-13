namespace RestExceptions.FluentValidation;

public static class RestExceptionsBuilderExtensions
{
    /// <summary>
    /// Maps FluentValidation's <c>ValidationException</c> using <see cref="ValidationExceptionMapper"/>.
    /// </summary>
    /// <param name="builder">The RestExceptions builder.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <code>
    /// builder.Services.AddRestExceptions(options => options.AddFluentValidation());
    /// </code>
    public static RestExceptionsBuilder AddFluentValidation(this RestExceptionsBuilder builder)
    {
        return builder.AddMapper<ValidationExceptionMapper>();
    }
}
