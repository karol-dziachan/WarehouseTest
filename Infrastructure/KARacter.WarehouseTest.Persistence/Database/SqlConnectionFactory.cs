using Microsoft.Data.SqlClient;
using KARacter.WarehouseTest.Application.Common.Interfaces.Factories;
using System.Data;

namespace KARacter.WarehouseTest.Persistence.Database;

public sealed class SqlConnectionFactory : ISqlConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    private SqlConnection? _connection;
    private bool _disposed;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        ValidateConnectionString(connectionString);
        _connectionString = connectionString;
    }

    public SqlConnection CreateConnection()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SqlConnectionFactory));

        try
        {
            _connection = new SqlConnection(_connectionString);

            // Ustawienia bezpieczeństwa
            _connection.AccessToken = null; // Wyczyść token dostępu jeśli był ustawiony
            _connection.Credential = null;  // Wyczyść poświadczenia jeśli były ustawione

            // Timeout i inne ustawienia
            _connection.StatisticsEnabled = true; // Włącz zbieranie statystyk

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }
        catch (SqlException ex)
        {
            _connection?.Dispose();
            throw new InvalidOperationException($"Failed to create SQL connection. Error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _connection?.Dispose();
            throw new InvalidOperationException("An unexpected error occurred while creating SQL connection.", ex);
        }
    }

    private static void ValidateConnectionString(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            // Sprawdź minimalne wymagane parametry
            if (string.IsNullOrEmpty(builder.DataSource))
                throw new ArgumentException("Data source is required in connection string.");

            if (string.IsNullOrEmpty(builder.InitialCatalog))
                throw new ArgumentException("Initial catalog is required in connection string.");

            // Sprawdź bezpieczeństwo
            if (!builder.IntegratedSecurity && string.IsNullOrEmpty(builder.UserID))
                throw new ArgumentException("Either Integrated Security must be true or User ID must be provided.");

            // Dodatkowe zabezpieczenia
            builder.Encrypt = true; // Wymuś szyfrowanie
            builder.TrustServerCertificate = false; // Nie ufaj niezaufanym certyfikatom
            builder.MultipleActiveResultSets = true; // Włącz MARS
            builder.ConnectTimeout = 30; // Timeout połączenia
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException("Invalid connection string format.", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_connection != null)
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
            _connection.Dispose();
            _connection = null;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~SqlConnectionFactory()
    {
        Dispose();
    }
}