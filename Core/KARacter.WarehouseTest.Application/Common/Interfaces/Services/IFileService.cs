namespace KARacter.WarehouseTest.Application.Common.Interfaces.Services;

public interface IFileService : IDisposable
{
    Task<string> DownloadAndSaveFileAsync(string url, string fileName, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> ReadCsvFileAsync<T>(string filePath, bool hasHeaderRecord = true, CancellationToken cancellationToken = default);
    IEnumerable<string> GetAllFiles();
}