namespace KARacter.WarehouseTest.Application.Common.Models.Configuration;

public class FileServiceOptions
{
    public const string SectionName = "FileService";

    public string BaseDirectory { get; init; } = "Data/Files";
    public int MaxFileSizeInMB { get; init; } = 300;
    public string[] AllowedExtensions { get; init; } = { ".csv" };
    public string[] AllowedContentTypes { get; init; } = { "text/csv", "application/csv" };
}