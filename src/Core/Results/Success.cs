namespace FlowRight.Core.Results;

public partial class Result
{
    #region Public Methods

    public static Result Success(ResultType resultType = ResultType.Success) =>
        new(resultType);

    public static Result<T> Success<T>(T value, ResultType resultType = ResultType.Success) =>
        new(value ?? throw new ArgumentNullException(nameof(value)), resultType);

    #endregion Public Methods
}