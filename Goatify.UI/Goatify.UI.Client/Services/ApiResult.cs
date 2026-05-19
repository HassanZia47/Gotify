namespace Goatify.UI.Services;

public sealed class ApiResult<T>
{
    private ApiResult(T? value, string? errorMessage, bool isSuccess, int? statusCode)
    {
        Value = value;
        ErrorMessage = errorMessage;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
    }

    public T? Value { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess { get; }
    public int? StatusCode { get; }

    public static ApiResult<T> Success(T? value, int? statusCode = null)
    {
        return new ApiResult<T>(value, null, true, statusCode);
    }

    public static ApiResult<T> Failure(string errorMessage, int? statusCode = null)
    {
        return new ApiResult<T>(default, errorMessage, false, statusCode);
    }
}
