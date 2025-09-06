using Microsoft.Extensions.DependencyInjection;

namespace FlowRight.Cqrs.Http;

/// <summary>
/// Interface for resolving services from the dependency injection container.
/// </summary>
public interface IServiceResolver
{
    /// <summary>
    /// Gets a required service of type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <returns>The resolved service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not registered.</exception>
    TService GetRequiredService<TService>() where TService : class;

    /// <summary>
    /// Gets an optional service of type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <returns>The resolved service, or null if not registered.</returns>
    TService? GetService<TService>() where TService : class;
}

/// <summary>
/// Default implementation of <see cref="IServiceResolver"/> that creates scopes for service resolution.
/// </summary>
/// <param name="serviceScopeFactory">The service scope factory to create scopes.</param>
public class ServiceResolver(IServiceScopeFactory serviceScopeFactory) : IServiceResolver
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    /// <inheritdoc />
    public TService GetRequiredService<TService>() where TService : class
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    /// <inheritdoc />
    public TService? GetService<TService>() where TService : class
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetService<TService>();
    }
}