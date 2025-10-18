namespace ERP_API.Common.Results;


public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Un resultado exitoso no puede tener un error");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Un resultado fallido debe tener un error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

   
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }
        return Success();
    }
}


public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool isSuccess, T? value, string? error) : base(isSuccess, error)
    {
        if (isSuccess && value == null)
            throw new InvalidOperationException("Un resultado exitoso debe tener un valor");

        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public new static Result<T> Failure(string error) => new(false, default, error);

   
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsFailure)
            return Result<TNew>.Failure(Error!);

        try
        {
            var newValue = mapper(Value!);
            return Result<TNew>.Success(newValue);
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure(ex.Message);
        }
    }

  
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);
        return this;
    }

  
    public Result<T> OnFailure(Action<string> action)
    {
        if (IsFailure)
            action(Error!);
        return this;
    }
}