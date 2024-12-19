namespace KARacter.WarehouseTest.Application.Features.DataProcessing.Commands;

public sealed record DataProcessingCommandResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = default!;
    public DateTime ProcessedAt { get; init; }

    private DataProcessingCommandResult(bool success, string message, DateTime processedAt)
    {
        Success = success;
        Message = message;
        ProcessedAt = processedAt;
    }

    public static DataProcessingCommandResult Succeeded(string message, DateTime processedAt)
        => new(true, message, processedAt);

    public static DataProcessingCommandResult Failed(string message, DateTime processedAt)
        => new(false, message, processedAt);
} 