using CsvHelper;
using CsvHelper.Configuration;
using KARacter.WarehouseTest.Application.Common.Interfaces.Services;
using KARacter.WarehouseTest.Application.Common.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security;
using System.Text;
using KARacter.WarehouseTest.Domain.Common.Interfaces;
using KARacter.WarehouseTest.Infrastructure.Constants;
using KARacter.WarehouseTest.Domain.Models.CsvModels;
using KARacter.WarehouseTest.Infrastructure.Mappings;

namespace KARacter.WarehouseTest.Infrastructure.Services;

public sealed class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FileServiceOptions _options;
    private bool _disposed;
    private const string HttpClientName = HttpClientNames.FileDownloader;

    public FileService(
        ILogger<FileService> logger,
        IHttpClientFactory httpClientFactory,
        IOptionsSnapshot<FileServiceOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));

        EnsureDirectoryExists(_options.BaseDirectory);
    }

    public async Task<string> DownloadAndSaveFileAsync(string url, string fileName, CancellationToken cancellationToken = default)
    {
        ValidateInput(url, fileName);

        try
        {
            _logger.LogInformation("Starting download of file {FileName} from {Url}", fileName, url);

            using var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            ValidateContentType(response);
            ValidateFileSize(response);

            var sanitizedFileName = SanitizeFileName(fileName);
            var filePath = GetSafeFilePath(sanitizedFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation("Successfully downloaded and saved file to {FilePath}", filePath);
            return filePath;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to download file from {Url}. Status code: {StatusCode}", url, ex.StatusCode);
            throw new InvalidOperationException($"Failed to download file from {url}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while downloading file from {Url}", url);
            throw;
        }
    }

    public IEnumerable<string> GetAllFiles()
    {
        try
        {
            _logger.LogInformation("Getting list of files from directory {Directory}", _options.BaseDirectory);

            if (!Directory.Exists(_options.BaseDirectory))
            {
                _logger.LogWarning("Directory {Directory} does not exist", _options.BaseDirectory);
                return Enumerable.Empty<string>();
            }

            var files = Directory.GetFiles(_options.BaseDirectory)
                               .Select(Path.GetFullPath)
                               .ToList();

            _logger.LogInformation("Found {Count} files in directory {Directory}",
                files.Count, _options.BaseDirectory);

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting list of files from directory {Directory}",
                _options.BaseDirectory);
            throw;
        }
    }

    public async Task<IEnumerable<T>> ReadCsvFileAsync<T>(string filePath, bool hasHeaderRecord = true, CancellationToken cancellationToken = default)
    {
        ValidateFilePath(filePath);

        try
        {
            _logger.LogInformation("Starting to read CSV file: {FilePath}", filePath);

            var config = GetCsvConfiguration<T>(hasHeaderRecord);
            var validRecords = new List<T>();

            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var csv = new CsvReader(reader, config);

            if (typeof(T) == typeof(string[]))
            {
                var records = new List<string[]>();
                while (await csv.ReadAsync())
                {
                    var record = csv.Parser.Record;
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }
                return records.Cast<T>();
            }

            if (typeof(T) == typeof(ProductCsvModel))
                csv.Context.RegisterClassMap<ProductMap>();
            else if (typeof(T) == typeof(PriceCsvModel))
                csv.Context.RegisterClassMap<PriceMap>();

            await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
            {
                try
                {
                    if (record is ProductCsvModel productModel)
                    {
                        if (!productModel.IsValid())
                        {
                            _logger.LogDebug("Skipping invalid product record. SKU: {SKU}", productModel.SKU);
                            continue;
                        }
                    }

                    ValidateRecord(record);
                    validRecords.Add(record);
                }
                catch (ValidationException ex)
                {
                    _logger.LogWarning("Skipping invalid record: {Message}", ex.Message);
                    continue;
                }
            }

            _logger.LogInformation("Successfully read {Count} valid records from {FilePath}", validRecords.Count, filePath);
            return validRecords;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading CSV file {FilePath}", filePath);
            throw;
        }
    }

    private static CsvConfiguration GetCsvConfiguration<T>(bool hasHeaderRecord)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeaderRecord,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
            Encoding = Encoding.UTF8,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true
        };

        if (typeof(T) == typeof(ProductCsvModel))
        {
            // Products: dane w osobnych komórkach
            config.Delimiter = ";";
            config.Quote = '"';
            config.PrepareHeaderForMatch = args => args.Header.Trim('"').ToLower();
            config.MissingFieldFound = null;
            config.HeaderValidated = null;
        }
        else if (typeof(T) == typeof(PriceCsvModel))
        {
            // Prices: dane w jednej komórce, przecinek jako separator
            config.Delimiter = ",";
            config.Quote = '"';
        }
        else
        {
            // Inventory: dane w jednej komórce, przecinek jako separator
            config.Delimiter = ",";
            config.Quote = '"';
        }

        return config;
    }

    private void ValidateInput(string url, string fileName)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid URL format", nameof(url));

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
            throw new ArgumentException($"Invalid file extension. Allowed extensions: {string.Join(", ", _options.AllowedExtensions)}");
    }

    private void ValidateContentType(HttpResponseMessage response)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType?.ToLower();
        if (string.IsNullOrEmpty(contentType) ||
            !_options.AllowedContentTypes.Contains(contentType))
        {
            throw new InvalidOperationException($"Invalid content type: {contentType}. Expected: {string.Join(", ", _options.AllowedContentTypes)}");
        }
    }

    private void ValidateFileSize(HttpResponseMessage response)
    {
        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength.HasValue && contentLength.Value > _options.MaxFileSizeInMB * 1024 * 1024)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {_options.MaxFileSizeInMB}MB");
        }
    }

    private void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var fullPath = Path.GetFullPath(filePath);
        if (!fullPath.StartsWith(_options.BaseDirectory, StringComparison.OrdinalIgnoreCase))
            throw new SecurityException("Access to file path outside base directory is not allowed");
    }

    private static void ValidateRecord<T>(T record)
    {
        if (record == null)
            throw new InvalidOperationException("Record cannot be null");

        if (record is IValidatable validatable)
            validatable.Validate();
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    private string GetSafeFilePath(string fileName)
    {
        var filePath = Path.Combine(_options.BaseDirectory, fileName);
        var directory = Path.GetDirectoryName(filePath);

        if (directory == null || !directory.StartsWith(_options.BaseDirectory, StringComparison.OrdinalIgnoreCase))
            throw new SecurityException("Invalid file path");

        return filePath;
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create directory: {path}", ex);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
