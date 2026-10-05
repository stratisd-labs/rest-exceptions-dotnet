using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RestExceptions;

/// <summary>
/// Configures RestExceptions, e.g. by registering additional <see cref="IRestExceptionMapper"/>s.
/// </summary>
/// <param name="services">The service collection RestExceptions is registered in.</param>
public sealed class RestExceptionsBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Registers an <see cref="IRestExceptionMapper"/>. Mappers run in registration order.
    /// Registering the same mapper type more than once has no effect.
    /// </summary>
    /// <typeparam name="TMapper">The mapper type.</typeparam>
    /// <returns>The builder, for chaining.</returns>
    public RestExceptionsBuilder AddMapper<TMapper>()
        where TMapper : class, IRestExceptionMapper
    {
        Services.TryAddEnumerable(ServiceDescriptor.Singleton<IRestExceptionMapper, TMapper>());
        return this;
    }
}
