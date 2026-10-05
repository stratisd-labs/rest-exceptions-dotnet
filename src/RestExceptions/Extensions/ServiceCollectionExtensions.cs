using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace RestExceptions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the RestExceptions services in the DI container.
    /// This includes the ProblemDetails service and the ExceptionHandler middleware.
    /// </summary>
    /// <remarks>
    /// It is important to note that the ExceptionHandler middleware must be registered as well, otherwise the exceptions will not be handled correctly.
    /// For instance, in Minimal APIs, you can register it like so:
    /// <br/>
    /// <code>app.UseExceptionHandler();</code>
    /// </remarks>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The newly configured <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddRestExceptions(this IServiceCollection services)
    {
        return services.AddRestExceptions(_ => { });
    }

    /// <summary>
    /// Registers the RestExceptions services in the DI container and configures them.
    /// </summary>
    /// <remarks>
    /// See <see cref="AddRestExceptions(IServiceCollection)"/> for the middleware that must be registered as well.
    /// </remarks>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configure">Configures RestExceptions, e.g. registers additional <see cref="IRestExceptionMapper"/>s.</param>
    /// <returns>The newly configured <see cref="IServiceCollection"/>.</returns>
    /// <code>
    /// builder.Services.AddRestExceptions(options => options.AddMapper&lt;CustomRestExceptionMapper&gt;());
    /// </code>
    public static IServiceCollection AddRestExceptions(
        this IServiceCollection services,
        Action<RestExceptionsBuilder> configure)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = context.HttpContext.Request.Path;
                context.ProblemDetails.Extensions.Add("method", context.HttpContext.Request.Method);
                context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
                context.ProblemDetails.Extensions.TryAdd("traceId",
                    context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity.Id);
            });
        services.AddExceptionHandler<RestExceptionHandler>();

        configure(new RestExceptionsBuilder(services));

        return services;
    }
}
