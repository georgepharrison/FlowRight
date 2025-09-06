namespace FlowRight.Cqrs.Http;

/// <summary>
/// Configuration class for managing the service resolver used by generated HTTP clients.
/// </summary>
public sealed class Config
{
    private static IServiceResolver? _serviceResolver;

    /// <summary>
    /// Gets or sets the service resolver used by generated HTTP clients to resolve dependencies.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing the property before initialization.</exception>
    /// <exception cref="ArgumentNullException">Thrown when setting the property to null.</exception>
    public static IServiceResolver ServiceResolver
    {
        get => _serviceResolver ?? throw new InvalidOperationException("ServiceResolver is not initialized.");
        set => _serviceResolver = value ?? throw new ArgumentNullException(nameof(value), "ServiceResolver cannot be null.");
    }
}