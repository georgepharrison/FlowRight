namespace FlowRight.Cqrs.Http;

/// <summary>
/// Interface for queries that return a result of type <typeparamref name="TResult"/>.
/// Queries represent read operations that don't change system state.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query.</typeparam>
public interface IQuery<TResult> : IRequest
{
}