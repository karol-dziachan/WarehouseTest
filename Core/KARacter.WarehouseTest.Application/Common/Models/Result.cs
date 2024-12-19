namespace KARacter.WarehouseTest.Application.Common.Models;

public sealed record Result<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = default!;
    public T? Data { get; init; }

    private Result(bool success, string message, T? data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    public static Result<T> Succeeded(T data, string message = "Operation completed successfully")
        => new(true, message, data);

    public static Result<T> Failed(string message)
        => new(false, message);
} 