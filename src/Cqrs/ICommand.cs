namespace FlowRight.Cqrs.Http;

/// <summary>
/// Marker interface for commands that don't return a result.
/// Commands represent write operations that change system state.
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Interface for commands that return a result of type <typeparamref name="TResult"/>.
/// Commands represent write operations that change system state and return data.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command.</typeparam>
public interface ICommand<TResult> : IRequest
{
}